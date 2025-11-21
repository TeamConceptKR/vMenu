using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuClient.Localization;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeaponLoadouts
    {
        // Variables
        private Menu menu = null;
        private readonly Menu SavedLoadoutsMenu = new(GetString("WeaponLoadouts_SavedLoadouts"), GetString("WeaponLoadouts_SavedLoadoutsList"));
        private readonly Menu ManageLoadoutMenu = new(GetString("WeaponLoadouts_ManageLoadout"), GetString("WeaponLoadouts_ManageLoadout_Desc"));
        public bool WeaponLoadoutsSetLoadoutOnRespawn { get; private set; } = UserDefaults.WeaponLoadoutsSetLoadoutOnRespawn;

        private readonly Dictionary<string, List<ValidWeapon>> SavedWeapons = new();

        public static Dictionary<string, List<ValidWeapon>> GetSavedWeapons()
        {
            var handle = StartFindKvp("vmenu_string_saved_weapon_loadout_");
            var saves = new Dictionary<string, List<ValidWeapon>>();
            while (true)
            {
                var kvp = FindKvp(handle);
                if (string.IsNullOrEmpty(kvp))
                {
                    break;
                }
                saves.Add(kvp, JsonConvert.DeserializeObject<List<ValidWeapon>>(GetResourceKvpString(kvp)));
            }
            EndFindKvp(handle);
            return saves;
        }

        private string SelectedSavedLoadoutName { get; set; } = "";
        // vmenu_temp_weapons_loadout_before_respawn
        // vmenu_string_saved_weapon_loadout_

        /// <summary>
        /// Returns the saved weapons list, as well as sets the <see cref="SavedWeapons"/> variable.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<ValidWeapon>> RefreshSavedWeaponsList()
        {
            if (SavedWeapons.Count > 0)
            {
                SavedWeapons.Clear();
            }

            var handle = StartFindKvp("vmenu_string_saved_weapon_loadout_");
            var saves = new List<string>();
            while (true)
            {
                var kvp = FindKvp(handle);
                if (string.IsNullOrEmpty(kvp))
                {
                    break;
                }
                saves.Add(kvp);
            }
            EndFindKvp(handle);

            foreach (var save in saves)
            {
                SavedWeapons.Add(save, JsonConvert.DeserializeObject<List<ValidWeapon>>(GetResourceKvpString(save)));
            }

            return SavedWeapons;
        }

        /// <summary>
        /// Creates the menu if it doesn't exist yet and sets the event handlers.
        /// </summary>
        public void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, GetString("WeaponLoadouts_Title"));

            MenuController.AddSubmenu(menu, SavedLoadoutsMenu);
            MenuController.AddSubmenu(SavedLoadoutsMenu, ManageLoadoutMenu);

            var saveLoadout = new MenuItem(GetString("WeaponLoadouts_SaveLoadout"), GetString("WeaponLoadouts_SaveLoadout_Desc"));
            var savedLoadoutsMenuBtn = new MenuItem(GetString("WeaponLoadouts_ManageLoadouts"), GetString("WeaponLoadouts_ManageLoadouts_Desc")) { Label = GetString("Common_Label_Arrow") };
            var enableDefaultLoadouts = new MenuCheckboxItem(GetString("WeaponLoadouts_RestoreDefault"), GetString("WeaponLoadouts_RestoreDefault_Desc"), WeaponLoadoutsSetLoadoutOnRespawn);

            menu.AddMenuItem(saveLoadout);
            menu.AddMenuItem(savedLoadoutsMenuBtn);
            MenuController.BindMenuItem(menu, SavedLoadoutsMenu, savedLoadoutsMenuBtn);
            if (IsAllowed(Permission.WLEquipOnRespawn))
            {
                menu.AddMenuItem(enableDefaultLoadouts);

                menu.OnCheckboxChange += (sender, checkbox, index, _checked) =>
                {
                    WeaponLoadoutsSetLoadoutOnRespawn = _checked;
                };
            }


            void RefreshSavedWeaponsMenu()
            {
                var oldCount = SavedLoadoutsMenu.Size;
                SavedLoadoutsMenu.ClearMenuItems(true);

                RefreshSavedWeaponsList();

                foreach (var sw in SavedWeapons)
                {
                    var btn = new MenuItem(sw.Key.Replace("vmenu_string_saved_weapon_loadout_", ""), GetString("WeaponLoadouts_ManageButton")) { Label = GetString("Common_Label_Arrow") };
                    SavedLoadoutsMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(SavedLoadoutsMenu, ManageLoadoutMenu, btn);
                }

                if (oldCount > SavedWeapons.Count)
                {
                    SavedLoadoutsMenu.RefreshIndex();
                }
            }


            var spawnLoadout = new MenuItem(GetString("WeaponLoadouts_EquipLoadout"), GetString("WeaponLoadouts_EquipLoadout_Desc"));
            var renameLoadout = new MenuItem(GetString("WeaponLoadouts_RenameLoadout"), GetString("WeaponLoadouts_RenameLoadout_Desc"));
            var cloneLoadout = new MenuItem(GetString("WeaponLoadouts_CloneLoadout"), GetString("WeaponLoadouts_CloneLoadout_Desc"));
            var setDefaultLoadout = new MenuItem(GetString("WeaponLoadouts_SetDefaultLoadout"), GetString("WeaponLoadouts_SetDefaultLoadout_Desc"));
            var replaceLoadout = new MenuItem(GetString("WeaponLoadouts_ReplaceLoadout"), GetString("WeaponLoadouts_ReplaceLoadout_Desc"));
            var deleteLoadout = new MenuItem(GetString("WeaponLoadouts_DeleteLoadout"), GetString("WeaponLoadouts_DeleteLoadout_Desc"));

            if (IsAllowed(Permission.WLEquip))
            {
                ManageLoadoutMenu.AddMenuItem(spawnLoadout);
            }

            ManageLoadoutMenu.AddMenuItem(renameLoadout);
            ManageLoadoutMenu.AddMenuItem(cloneLoadout);
            ManageLoadoutMenu.AddMenuItem(setDefaultLoadout);
            ManageLoadoutMenu.AddMenuItem(replaceLoadout);
            ManageLoadoutMenu.AddMenuItem(deleteLoadout);

            // Save the weapons loadout.
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == saveLoadout)
                {
                    var name = await GetUserInput(GetString("WeaponLoadouts_EnterSaveName"), 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (SavedWeapons.ContainsKey("vmenu_string_saved_weapon_loadout_" + name))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            if (SaveWeaponLoadout("vmenu_string_saved_weapon_loadout_" + name))
                            {
                                Log("saveweapons called from menu select (save loadout button)");
                                Notify.Success(GetString("WeaponLoadouts_SaveSuccess", name));
                            }
                            else
                            {
                                Notify.Error(CommonErrors.UnknownError);
                            }
                        }
                    }
                }
            };

            // manage spawning, renaming, deleting etc.
            ManageLoadoutMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (SavedWeapons.ContainsKey(SelectedSavedLoadoutName))
                {
                    var weapons = SavedWeapons[SelectedSavedLoadoutName];

                    if (item == spawnLoadout) // spawn
                    {
                        await SpawnWeaponLoadoutAsync(SelectedSavedLoadoutName, false, true, false);
                    }
                    else if (item == renameLoadout || item == cloneLoadout) // rename or clone
                    {
                        var newName = await GetUserInput(GetString("WeaponLoadouts_EnterSaveName"), SelectedSavedLoadoutName.Replace("vmenu_string_saved_weapon_loadout_", ""), 30);
                        if (string.IsNullOrEmpty(newName))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                        else
                        {
                            if (SavedWeapons.ContainsKey("vmenu_string_saved_weapon_loadout_" + newName))
                            {
                                Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            }
                            else
                            {
                                SetResourceKvp("vmenu_string_saved_weapon_loadout_" + newName, JsonConvert.SerializeObject(weapons));
                                Notify.Success(GetString(item == renameLoadout ? "WeaponLoadouts_RenameSuccess" : "WeaponLoadouts_CloneSuccess", newName));

                                if (item == renameLoadout)
                                {
                                    DeleteResourceKvp(SelectedSavedLoadoutName);
                                }

                                ManageLoadoutMenu.GoBack();
                            }
                        }
                    }
                    else if (item == setDefaultLoadout) // set as default
                    {
                        SetResourceKvp("vmenu_string_default_loadout", SelectedSavedLoadoutName);
                        Notify.Success(GetString("WeaponLoadouts_DefaultSuccess"));
                        item.LeftIcon = MenuItem.Icon.TICK;
                    }
                    else if (item == replaceLoadout) // replace
                    {
                        if (replaceLoadout.Label == GetString("BannedPlayers_AreYouSure"))
                        {
                            replaceLoadout.Label = "";
                            SaveWeaponLoadout(SelectedSavedLoadoutName);
                            Log("save weapons called from replace loadout");
                            Notify.Success(GetString("WeaponLoadouts_ReplaceSuccess"));
                        }
                        else
                        {
                            replaceLoadout.Label = GetString("BannedPlayers_AreYouSure");
                        }
                    }
                    else if (item == deleteLoadout) // delete
                    {
                        if (deleteLoadout.Label == GetString("BannedPlayers_AreYouSure"))
                        {
                            deleteLoadout.Label = "";
                            DeleteResourceKvp(SelectedSavedLoadoutName);
                            ManageLoadoutMenu.GoBack();
                            Notify.Success(GetString("WeaponLoadouts_DeleteSuccess"));
                        }
                        else
                        {
                            deleteLoadout.Label = GetString("BannedPlayers_AreYouSure");
                        }
                    }
                }
            };

            // Reset the 'are you sure' states.
            ManageLoadoutMenu.OnMenuClose += (sender) =>
            {
                deleteLoadout.Label = "";
                renameLoadout.Label = "";
            };
            // Reset the 'are you sure' states.
            ManageLoadoutMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                deleteLoadout.Label = "";
                renameLoadout.Label = "";
            };

            // Refresh the spawned weapons menu whenever this menu is opened.
            SavedLoadoutsMenu.OnMenuOpen += (sender) =>
            {
                RefreshSavedWeaponsMenu();
            };

            // Set the current saved loadout whenever a loadout is selected.
            SavedLoadoutsMenu.OnItemSelect += (sender, item, index) =>
            {
                if (SavedWeapons.ContainsKey("vmenu_string_saved_weapon_loadout_" + item.Text))
                {
                    SelectedSavedLoadoutName = "vmenu_string_saved_weapon_loadout_" + item.Text;
                }
                else // shouldn't ever happen, but just in case
                {
                    ManageLoadoutMenu.GoBack();
                }
            };

            // Reset the index whenever the ManageLoadout menu is opened. Just to prevent auto selecting the delete option for example.
            ManageLoadoutMenu.OnMenuOpen += (sender) =>
            {
                ManageLoadoutMenu.RefreshIndex();
                var kvp = GetResourceKvpString("vmenu_string_default_loadout");
                if (string.IsNullOrEmpty(kvp) || kvp != SelectedSavedLoadoutName)
                {
                    setDefaultLoadout.LeftIcon = MenuItem.Icon.NONE;
                }
                else
                {
                    setDefaultLoadout.LeftIcon = MenuItem.Icon.TICK;
                }

            };

            // Refresh the saved weapons menu.
            RefreshSavedWeaponsMenu();
        }

        /// <summary>
        /// Gets the menu.
        /// </summary>
        /// <returns></returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
