using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class TimeOptions
    {
        // Variables
        private Menu menu;
        public MenuItem freezeTimeToggle;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, Localization.GetString("TimeOptions_Title"));

            // Create all menu items.
            freezeTimeToggle = new MenuItem(Localization.GetString("TimeOptions_FreezeTime"), Localization.GetString("TimeOptions_FreezeTime_Desc"));
            var earlymorning = new MenuItem(Localization.GetString("TimeOptions_EarlyMorning"), Localization.GetString("TimeOptions_EarlyMorning_Desc"))
            {
                Label = "06:00"
            };
            var morning = new MenuItem(Localization.GetString("TimeOptions_Morning"), Localization.GetString("TimeOptions_Morning_Desc"))
            {
                Label = "09:00"
            };
            var noon = new MenuItem(Localization.GetString("TimeOptions_Noon"), Localization.GetString("TimeOptions_Noon_Desc"))
            {
                Label = "12:00"
            };
            var earlyafternoon = new MenuItem(Localization.GetString("TimeOptions_EarlyAfternoon"), Localization.GetString("TimeOptions_EarlyAfternoon_Desc"))
            {
                Label = "15:00"
            };
            var afternoon = new MenuItem(Localization.GetString("TimeOptions_Afternoon"), Localization.GetString("TimeOptions_Afternoon_Desc"))
            {
                Label = "18:00"
            };
            var evening = new MenuItem(Localization.GetString("TimeOptions_Evening"), Localization.GetString("TimeOptions_Evening_Desc"))
            {
                Label = "21:00"
            };
            var midnight = new MenuItem(Localization.GetString("TimeOptions_Midnight"), Localization.GetString("TimeOptions_Midnight_Desc"))
            {
                Label = "00:00"
            };
            var night = new MenuItem(Localization.GetString("TimeOptions_Night"), Localization.GetString("TimeOptions_Night_Desc"))
            {
                Label = "03:00"
            };

            var hours = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            var minutes = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (var i = 10; i < 60; i++)
            {
                if (i < 24)
                {
                    hours.Add(i.ToString());
                }
                minutes.Add(i.ToString());
            }
            var manualHour = new MenuListItem(Localization.GetString("TimeOptions_SetCustomHour"), hours, 0);
            var manualMinute = new MenuListItem(Localization.GetString("TimeOptions_SetCustomMinute"), minutes, 0);

            // Add all menu items to the menu.
            if (IsAllowed(Permission.TOFreezeTime))
            {
                menu.AddMenuItem(freezeTimeToggle);
            }
            if (IsAllowed(Permission.TOSetTime))
            {
                menu.AddMenuItem(earlymorning);
                menu.AddMenuItem(morning);
                menu.AddMenuItem(noon);
                menu.AddMenuItem(earlyafternoon);
                menu.AddMenuItem(afternoon);
                menu.AddMenuItem(evening);
                menu.AddMenuItem(midnight);
                menu.AddMenuItem(night);
                menu.AddMenuItem(manualHour);
                menu.AddMenuItem(manualMinute);
            }

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If it's the freeze time button.
                if (item == freezeTimeToggle)
                {
                    Subtitle.Info(Localization.GetString("TimeOptions_TimeFreezeInfo", 
                        EventManager.IsServerTimeFrozen ? Localization.GetString("TimeOptions_Continue") : Localization.GetString("TimeOptions_Freeze")), 
                        prefix: "Info:");
                    FreezeServerTime(!EventManager.IsServerTimeFrozen);
                }
                else
                {
                    // Set the time using the index and some math :)
                    // eg: index = 3 (12:00) ---> 3 * 3 (=9) + 3 [= 12] ---> 12:00
                    // eg: index = 8 (03:00) ---> 8 * 3 (=24) + 3 (=27, >23 so 27-24) [=3] ---> 03:00
                    var newHour = 0;
                    if (IsAllowed(Permission.TOFreezeTime))
                    {
                        newHour = (index * 3) + 3 < 23 ? (index * 3) + 3 : (index * 3) + 3 - 24;
                    }
                    else
                    {
                        newHour = ((index + 1) * 3) + 3 < 23 ? ((index + 1) * 3) + 3 : ((index + 1) * 3) + 3 - 24;
                    }

                    var newMinute = 0;
                    Subtitle.Info(Localization.GetString("TimeOptions_TimeSetInfo",
                        newHour < 10 ? $"0{newHour}" : newHour.ToString(),
                        newMinute < 10 ? $"0{newMinute}" : newMinute.ToString()), 
                        prefix: "Info:");
                    UpdateServerTime(newHour, newMinute);
                }

            };

            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                var newHour = EventManager.GetServerHours;
                var newMinute = EventManager.GetServerMinutes;
                if (item == manualHour)
                {
                    newHour = item.ListIndex;
                }
                else if (item == manualMinute)
                {
                    newMinute = item.ListIndex;
                }

                Subtitle.Info(Localization.GetString("TimeOptions_TimeSetInfo",
                    newHour < 10 ? $"0{newHour}" : newHour.ToString(),
                    newMinute < 10 ? $"0{newMinute}" : newMinute.ToString()), 
                    prefix: "Info:");
                UpdateServerTime(newHour, newMinute);
            };
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
    }
}
