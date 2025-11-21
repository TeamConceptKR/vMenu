using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using vMenuShared;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeatherOptions
    {
        // Variables
        private Menu menu;
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;
        public MenuCheckboxItem vehicleBlackout;
        public MenuCheckboxItem snowEnabled;
        public static readonly List<string> weatherTypes = new()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };
        
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, Localization.GetString("WeatherOptions_Title"));

            dynamicWeatherEnabled = new MenuCheckboxItem(Localization.GetString("WeatherOptions_DynamicWeather"), Localization.GetString("WeatherOptions_DynamicWeather_Desc"), EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem(Localization.GetString("WeatherOptions_Blackout"), Localization.GetString("WeatherOptions_Blackout_Desc"), EventManager.IsBlackoutEnabled);
            vehicleBlackout = new MenuCheckboxItem(Localization.GetString("WeatherOptions_VehicleBlackout"), Localization.GetString("WeatherOptions_VehicleBlackout_Desc"), !EventManager.IsVehicleLightsEnabled);
            snowEnabled = new MenuCheckboxItem(Localization.GetString("WeatherOptions_SnowEffects"), Localization.GetString("WeatherOptions_SnowEffects_Desc"), ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_enable_snow));
            
            var extrasunny = new MenuItem(Localization.GetString("WeatherOptions_ExtraSunny"), Localization.GetString("WeatherOptions_ExtraSunny_Desc")) { ItemData = "EXTRASUNNY" };
            var clear = new MenuItem(Localization.GetString("WeatherOptions_Clear"), Localization.GetString("WeatherOptions_Clear_Desc")) { ItemData = "CLEAR" };
            var neutral = new MenuItem(Localization.GetString("WeatherOptions_Neutral"), Localization.GetString("WeatherOptions_Neutral_Desc")) { ItemData = "NEUTRAL" };
            var smog = new MenuItem(Localization.GetString("WeatherOptions_Smog"), Localization.GetString("WeatherOptions_Smog_Desc")) { ItemData = "SMOG" };
            var foggy = new MenuItem(Localization.GetString("WeatherOptions_Foggy"), Localization.GetString("WeatherOptions_Foggy_Desc")) { ItemData = "FOGGY" };
            var clouds = new MenuItem(Localization.GetString("WeatherOptions_Cloudy"), Localization.GetString("WeatherOptions_Cloudy_Desc")) { ItemData = "CLOUDS" };
            var overcast = new MenuItem(Localization.GetString("WeatherOptions_Overcast"), Localization.GetString("WeatherOptions_Overcast_Desc")) { ItemData = "OVERCAST" };
            var clearing = new MenuItem(Localization.GetString("WeatherOptions_Clearing"), Localization.GetString("WeatherOptions_Clearing_Desc")) { ItemData = "CLEARING" };
            var rain = new MenuItem(Localization.GetString("WeatherOptions_Rainy"), Localization.GetString("WeatherOptions_Rainy_Desc")) { ItemData = "RAIN" };
            var thunder = new MenuItem(Localization.GetString("WeatherOptions_Thunder"), Localization.GetString("WeatherOptions_Thunder_Desc")) { ItemData = "THUNDER" };
            var blizzard = new MenuItem(Localization.GetString("WeatherOptions_Blizzard"), Localization.GetString("WeatherOptions_Blizzard_Desc")) { ItemData = "BLIZZARD" };
            var snow = new MenuItem(Localization.GetString("WeatherOptions_Snow"), Localization.GetString("WeatherOptions_Snow_Desc")) { ItemData = "SNOW" };
            var snowlight = new MenuItem(Localization.GetString("WeatherOptions_LightSnow"), Localization.GetString("WeatherOptions_LightSnow_Desc")) { ItemData = "SNOWLIGHT" };
            var xmas = new MenuItem(Localization.GetString("WeatherOptions_XMasSnow"), Localization.GetString("WeatherOptions_XMasSnow_Desc")) { ItemData = "XMAS" };
            var halloween = new MenuItem(Localization.GetString("WeatherOptions_Halloween"), Localization.GetString("WeatherOptions_Halloween_Desc")) { ItemData = "HALLOWEEN" };
            var removeclouds = new MenuItem(Localization.GetString("WeatherOptions_RemoveClouds"), Localization.GetString("WeatherOptions_RemoveClouds_Desc"));
            var randomizeclouds = new MenuItem(Localization.GetString("WeatherOptions_RandomizeClouds"), Localization.GetString("WeatherOptions_RandomizeClouds_Desc"));

            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
            }
            if (IsAllowed(Permission.WOVehBlackout))
            {
                menu.AddMenuItem(vehicleBlackout);
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                menu.AddMenuItem(snowEnabled);
                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            menu.OnItemSelect += (sender, item, index2) =>
            {
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
                else if (item.ItemData is string weatherType)
                {
                    Notify.Custom(Localization.GetString("WeatherOptions_WeatherChange", item.Text, EventManager.WeatherChangeTime));
                    UpdateServerWeather(weatherType, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom(Localization.GetString("WeatherOptions_DynamicToggle", _checked ? Localization.GetString("Common_Enabled") : Localization.GetString("Common_Disabled")));
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.IsSnowEnabled);
                }
                else if (item == blackout)
                {
                    Notify.Custom(Localization.GetString("WeatherOptions_BlackoutToggle", _checked ? Localization.GetString("Common_Enabled") : Localization.GetString("Common_Disabled")));
                    UpdateServerBlackout(_checked);
                }
                else if (item == vehicleBlackout)
                {
                    Notify.Custom(Localization.GetString("WeatherOptions_VehicleBlackoutToggle", _checked ? Localization.GetString("Common_Enabled") : Localization.GetString("Common_Disabled")));
                    UpdateServerVehicleBlackout(!_checked);
                }
                else if (item == snowEnabled)
                {
                    if (EventManager.GetServerWeather is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD")
                    {
                        Notify.Custom(Localization.GetString("WeatherOptions_SnowDisableWarning", EventManager.GetServerWeather));
                        return;
                    }

                    Notify.Custom(Localization.GetString("WeatherOptions_SnowToggle", _checked ? Localization.GetString("Common_Enabled") : Localization.GetString("Common_Disabled")));
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.DynamicWeatherEnabled, _checked);
                }
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
