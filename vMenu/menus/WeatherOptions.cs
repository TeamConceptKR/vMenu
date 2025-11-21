using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using vMenuShared;

using static vMenuClient.CommonFunctions;
using static vMenuClient.Localization;
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
            menu = new Menu(Game.Player.Name, GetString("WeatherOptions_Title"));

            dynamicWeatherEnabled = new MenuCheckboxItem(GetString("WeatherOptions_DynamicWeather"), GetString("WeatherOptions_DynamicWeather_Desc"), EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem(GetString("WeatherOptions_Blackout"), GetString("WeatherOptions_Blackout_Desc"), EventManager.IsBlackoutEnabled);
            vehicleBlackout = new MenuCheckboxItem(GetString("WeatherOptions_VehicleBlackout"), GetString("WeatherOptions_VehicleBlackout_Desc"), !EventManager.IsVehicleLightsEnabled);
            snowEnabled = new MenuCheckboxItem(GetString("WeatherOptions_SnowEffects"), GetString("WeatherOptions_SnowEffects_Desc"), ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_enable_snow));
            
            var extrasunny = new MenuItem(GetString("WeatherOptions_ExtraSunny"), GetString("WeatherOptions_ExtraSunny_Desc")) { ItemData = "EXTRASUNNY" };
            var clear = new MenuItem(GetString("WeatherOptions_Clear"), GetString("WeatherOptions_Clear_Desc")) { ItemData = "CLEAR" };
            var neutral = new MenuItem(GetString("WeatherOptions_Neutral"), GetString("WeatherOptions_Neutral_Desc")) { ItemData = "NEUTRAL" };
            var smog = new MenuItem(GetString("WeatherOptions_Smog"), GetString("WeatherOptions_Smog_Desc")) { ItemData = "SMOG" };
            var foggy = new MenuItem(GetString("WeatherOptions_Foggy"), GetString("WeatherOptions_Foggy_Desc")) { ItemData = "FOGGY" };
            var clouds = new MenuItem(GetString("WeatherOptions_Cloudy"), GetString("WeatherOptions_Cloudy_Desc")) { ItemData = "CLOUDS" };
            var overcast = new MenuItem(GetString("WeatherOptions_Overcast"), GetString("WeatherOptions_Overcast_Desc")) { ItemData = "OVERCAST" };
            var clearing = new MenuItem(GetString("WeatherOptions_Clearing"), GetString("WeatherOptions_Clearing_Desc")) { ItemData = "CLEARING" };
            var rain = new MenuItem(GetString("WeatherOptions_Rainy"), GetString("WeatherOptions_Rainy_Desc")) { ItemData = "RAIN" };
            var thunder = new MenuItem(GetString("WeatherOptions_Thunder"), GetString("WeatherOptions_Thunder_Desc")) { ItemData = "THUNDER" };
            var blizzard = new MenuItem(GetString("WeatherOptions_Blizzard"), GetString("WeatherOptions_Blizzard_Desc")) { ItemData = "BLIZZARD" };
            var snow = new MenuItem(GetString("WeatherOptions_Snow"), GetString("WeatherOptions_Snow_Desc")) { ItemData = "SNOW" };
            var snowlight = new MenuItem(GetString("WeatherOptions_LightSnow"), GetString("WeatherOptions_LightSnow_Desc")) { ItemData = "SNOWLIGHT" };
            var xmas = new MenuItem(GetString("WeatherOptions_XMasSnow"), GetString("WeatherOptions_XMasSnow_Desc")) { ItemData = "XMAS" };
            var halloween = new MenuItem(GetString("WeatherOptions_Halloween"), GetString("WeatherOptions_Halloween_Desc")) { ItemData = "HALLOWEEN" };
            var removeclouds = new MenuItem(GetString("WeatherOptions_RemoveClouds"), GetString("WeatherOptions_RemoveClouds_Desc"));
            var randomizeclouds = new MenuItem(GetString("WeatherOptions_RandomizeClouds"), GetString("WeatherOptions_RandomizeClouds_Desc"));

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
                    Notify.Custom(GetString("WeatherOptions_WeatherChange", item.Text, EventManager.WeatherChangeTime));
                    UpdateServerWeather(weatherType, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom(GetString("WeatherOptions_DynamicToggle", _checked ? GetString("Common_Enabled") : GetString("Common_Disabled")));
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.IsSnowEnabled);
                }
                else if (item == blackout)
                {
                    Notify.Custom(GetString("WeatherOptions_BlackoutToggle", _checked ? GetString("Common_Enabled") : GetString("Common_Disabled")));
                    UpdateServerBlackout(_checked);
                }
                else if (item == vehicleBlackout)
                {
                    Notify.Custom(GetString("WeatherOptions_VehicleBlackoutToggle", _checked ? GetString("Common_Enabled") : GetString("Common_Disabled")));
                    UpdateServerVehicleBlackout(!_checked);
                }
                else if (item == snowEnabled)
                {
                    if (EventManager.GetServerWeather is "XMAS" or "SNOWLIGHT" or "SNOW" or "BLIZZARD")
                    {
                        Notify.Custom(GetString("WeatherOptions_SnowDisableWarning", EventManager.GetServerWeather));
                        return;
                    }

                    Notify.Custom(GetString("WeatherOptions_SnowToggle", _checked ? GetString("Common_Enabled") : GetString("Common_Disabled")));
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
