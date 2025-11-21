using MenuAPI;

namespace vMenuClient.menus
{
    public class About
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu("vMenu", "vMenu 정보");

            // Create menu items.
            var version = new MenuItem("vMenu 버전", $"이 서버는 vMenu ~b~~h~{MainMenu.Version}~h~~s~ 버전을 사용합니다.")
            {
                Label = $"~h~{MainMenu.Version}~h~"
            };
            var credits = new MenuItem("vMenu 정보 / 제작진", "vMenu는 ~b~Vespura~s~가 제작했습니다. 자세한 정보는 ~b~www.vespura.com/vmenu~s~을 확인하세요. 기여해주신 Deltanic, Brigliar, IllusiveTea, Shayan Doust, zr0iq, Golden 님께 감사드립니다!");

            var serverInfoMessage = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_message);
            if (!string.IsNullOrEmpty(serverInfoMessage))
            {
                var serverInfo = new MenuItem("서버 정보", serverInfoMessage);
                var siteUrl = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_website_url);
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    serverInfo.Label = $"{siteUrl}";
                }
                menu.AddMenuItem(serverInfo);
            }
            menu.AddMenuItem(version);
            menu.AddMenuItem(credits);
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
