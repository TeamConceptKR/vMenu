using System;
using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static vMenuClient.CommonFunctions;
using static vMenuClient.Localization;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class BannedPlayers
    {
        // Variables
        private Menu menu;

        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        public struct BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
            public string uuid;
        }

        BanRecord currentRecord = new();

        public List<BanRecord> banlist = new();

        readonly Menu bannedPlayer = new(GetString("BannedPlayers_BannedPlayer"), GetString("BannedPlayers_BanRecord"));

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, GetString("BannedPlayers_Title"));

            menu.InstructionalButtons.Add(Control.Jump, GetString("BannedPlayers_FilterOptions"));
            menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(async (a, b) =>
            {
                if (banlist.Count > 1)
                {
                    var filterText = await GetUserInput(GetString("BannedPlayers_FilterPrompt"));
                    if (string.IsNullOrEmpty(filterText))
                    {
                        Subtitle.Custom(GetString("BannedPlayers_FiltersCleared"));
                        menu.ResetFilter();
                        UpdateBans();
                    }
                    else
                    {
                        menu.FilterMenuItems(item => item.ItemData is BanRecord br && (br.playerName.ToLower().Contains(filterText.ToLower()) || br.uuid.ToLower().Contains(filterText.ToLower())));
                        Subtitle.Custom(GetString("BannedPlayers_FilterApplied"));
                    }
                }
                else
                {
                    Notify.Error(GetString("BannedPlayers_FilterError"));
                }

                Log($"Button pressed: {a} {b}");
            }), true));

            bannedPlayer.AddMenuItem(new MenuItem(GetString("BannedPlayers_PlayerName")));
            bannedPlayer.AddMenuItem(new MenuItem(GetString("BannedPlayers_BannedBy")));
            bannedPlayer.AddMenuItem(new MenuItem(GetString("BannedPlayers_BannedUntil")));
            bannedPlayer.AddMenuItem(new MenuItem(GetString("BannedPlayers_PlayerIdentifiers")));
            bannedPlayer.AddMenuItem(new MenuItem(GetString("BannedPlayers_BannedFor")));
            bannedPlayer.AddMenuItem(new MenuItem(GetString("BannedPlayers_Unban"), GetString("BannedPlayers_Unban_Desc")));

            // should be enough for now to cover all possible identifiers.
            var colors = new List<string>() { "~r~", "~g~", "~b~", "~o~", "~y~", "~p~", "~s~", "~t~", };

            bannedPlayer.OnMenuClose += (sender) =>
            {
                BaseScript.TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                bannedPlayer.GetMenuItems()[5].Label = "";
                UpdateBans();
            };

            bannedPlayer.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                bannedPlayer.GetMenuItems()[5].Label = "";
            };

            bannedPlayer.OnItemSelect += (sender, item, index) =>
            {
                if (index == 5 && IsAllowed(Permission.OPUnban))
                {
                    if (item.Label == GetString("BannedPlayers_AreYouSure"))
                    {
                        if (banlist.Contains(currentRecord))
                        {
                            UnbanPlayer(banlist.IndexOf(currentRecord));
                            bannedPlayer.GetMenuItems()[5].Label = "";
                            bannedPlayer.GoBack();
                        }
                        else
                        {
                            Notify.Error(GetString("BannedPlayers_UnbanError"));
                        }
                    }
                    else
                    {
                        item.Label = GetString("BannedPlayers_AreYouSure");
                    }
                }
                else
                {
                    bannedPlayer.GetMenuItems()[5].Label = "";
                }

            };

            menu.OnItemSelect += (sender, item, index) =>
            {
                currentRecord = item.ItemData;

                bannedPlayer.MenuSubtitle = GetString("BannedPlayers_BanRecordSubtitle", currentRecord.playerName);
                var nameItem = bannedPlayer.GetMenuItems()[0];
                var bannedByItem = bannedPlayer.GetMenuItems()[1];
                var bannedUntilItem = bannedPlayer.GetMenuItems()[2];
                var playerIdentifiersItem = bannedPlayer.GetMenuItems()[3];
                var banReasonItem = bannedPlayer.GetMenuItems()[4];
                nameItem.Label = currentRecord.playerName;
                nameItem.Description = GetString("BannedPlayers_PlayerNameDesc", currentRecord.playerName);
                bannedByItem.Label = currentRecord.bannedBy;
                bannedByItem.Description = GetString("BannedPlayers_BannedByDesc", currentRecord.bannedBy);
                if (currentRecord.bannedUntil.Date.Year == 3000)
                {
                    bannedUntilItem.Label = GetString("BannedPlayers_Forever");
                }
                else
                {
                    bannedUntilItem.Label = currentRecord.bannedUntil.Date.ToString();
                }

                bannedUntilItem.Description = GetString("BannedPlayers_BannedUntilDesc", currentRecord.bannedUntil.Date.ToString());
                playerIdentifiersItem.Description = "";

                var i = 0;
                foreach (var id in currentRecord.identifiers)
                {
                    // only (admins) people that can unban players are allowed to view IP's.
                    // this is just a slight 'safety' feature in case someone who doesn't know what they're doing
                    // gave builtin.everyone access to view the banlist.
                    if (id.StartsWith("ip:") && !IsAllowed(Permission.OPUnban))
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}{GetString("BannedPlayers_IPHidden")}";
                    }
                    else
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}{id.Replace(":", ": ")} ";
                    }
                    i++;
                }
                banReasonItem.Description = GetString("BannedPlayers_BannedForDesc", currentRecord.banReason);

                var unbanPlayerBtn = bannedPlayer.GetMenuItems()[5];
                unbanPlayerBtn.Label = "";
                if (!IsAllowed(Permission.OPUnban))
                {
                    unbanPlayerBtn.Enabled = false;
                    unbanPlayerBtn.Description = GetString("BannedPlayers_UnbanNotAllowed");
                    unbanPlayerBtn.LeftIcon = MenuItem.Icon.LOCK;
                }

                bannedPlayer.RefreshIndex();
            };
            MenuController.AddMenu(bannedPlayer);

        }

        /// <summary>
        /// Updates the ban list menu.
        /// </summary>
        public void UpdateBans()
        {
            menu.ResetFilter();
            menu.ClearMenuItems();

            foreach (var ban in banlist)
            {
                var recordBtn = new MenuItem(ban.playerName, GetString("BannedPlayers_RecordButton", ban.playerName, ban.bannedBy, ban.bannedUntil, ban.banReason))
                {
                    Label = GetString("Common_Label_Arrow"),
                    ItemData = ban
                };
                menu.AddMenuItem(recordBtn);
                MenuController.BindMenuItem(menu, bannedPlayer, recordBtn);
            }
            menu.RefreshIndex();
        }

        /// <summary>
        /// Updates the list of ban records.
        /// </summary>
        /// <param name="banJsonString"></param>
        public void UpdateBanList(string banJsonString)
        {
            banlist.Clear();
            banlist = JsonConvert.DeserializeObject<List<BanRecord>>(banJsonString);
            UpdateBans();
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }

            return menu;
        }

        /// <summary>
        /// Sends an event to the server requesting the player to be unbanned.
        /// We'll just assume that worked fine, so remove the item from our local list, we'll re-sync once the menu is re-opened.
        /// </summary>
        /// <param name="index"></param>
        private void UnbanPlayer(int index)
        {
            var record = banlist[index];
            banlist.Remove(record);
            BaseScript.TriggerServerEvent("vMenu:RequestPlayerUnban", record.uuid);
        }

        /// <summary>
        /// Converts the ban record (json object) into a BanRecord struct.
        /// </summary>
        /// <param name="banRecordJsonObject"></param>
        /// <returns></returns>
        public static BanRecord JsonToBanRecord(dynamic banRecordJsonObject)
        {
            var newBr = new BanRecord();
            foreach (Newtonsoft.Json.Linq.JProperty brValue in banRecordJsonObject)
            {
                var key = brValue.Name.ToString();
                var value = brValue.Value;
                if (key == "playerName")
                {
                    newBr.playerName = value.ToString();
                }
                else if (key == "identifiers")
                {
                    var tmpList = new List<string>();
                    foreach (var identifier in value)
                    {
                        tmpList.Add(identifier.ToString());
                    }
                    newBr.identifiers = tmpList;
                }
                else if (key == "bannedUntil")
                {
                    newBr.bannedUntil = DateTime.Parse(value.ToString());
                }
                else if (key == "banReason")
                {
                    newBr.banReason = value.ToString();
                }
                else if (key == "bannedBy")
                {
                    newBr.bannedBy = value.ToString();
                }
            }
            return newBr;
        }
    }
}
