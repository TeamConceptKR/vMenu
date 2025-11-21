using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuClient.Localization;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class Recording
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            AddTextEntryByHash(0x86F10CE6, GetString("Recording_UploadToCfxForum")); // Replace the "Upload To Social Club" button in gallery
            AddTextEntry("ERROR_UPLOAD", GetString("Recording_UploadConfirm")); // Replace the warning message text for uploading

            // Create the menu.
            menu = new Menu(GetString("Menu_RecordingOptions"), GetString("Recording_Title"));

            var takePic = new MenuItem(GetString("Recording_TakePhoto"), GetString("Recording_TakePhoto_Desc"));
            var openPmGallery = new MenuItem(GetString("Recording_OpenGallery"), GetString("Recording_OpenGallery_Desc"));
            var startRec = new MenuItem(GetString("Recording_StartRecording"), GetString("Recording_StartRecording_Desc"));
            var stopRec = new MenuItem(GetString("Recording_StopRecording"), GetString("Recording_StopRecording_Desc"));
            var openEditor = new MenuItem(GetString("Recording_RockstarEditor"), GetString("Recording_RockstarEditor_Desc"));

            menu.AddMenuItem(takePic);
            menu.AddMenuItem(openPmGallery);
            menu.AddMenuItem(startRec);
            menu.AddMenuItem(stopRec);
            menu.AddMenuItem(openEditor);

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == startRec)
                {
                    if (IsRecording())
                    {
                        Notify.Alert(GetString("Recording_AlreadyRecording"));
                    }
                    else
                    {
                        StartRecording(1);
                    }
                }
                else if (item == openPmGallery)
                {
                    ActivateFrontendMenu((uint)GetHashKey("FE_MENU_VERSION_MP_PAUSE"), true, 3);
                }
                else if (item == takePic)
                {
                    BeginTakeHighQualityPhoto();
                    SaveHighQualityPhoto(-1);
                    FreeMemoryForHighQualityPhoto();
                }
                else if (item == stopRec)
                {
                    if (!IsRecording())
                    {
                        Notify.Alert(GetString("Recording_NotRecording"));
                    }
                    else
                    {
                        StopRecordingAndSaveClip();
                    }
                }
                else if (item == openEditor)
                {
                    if (GetSettingsBool(Setting.vmenu_quit_session_in_rockstar_editor))
                    {
                        QuitSession();
                    }
                    ActivateRockstarEditor();
                    // wait for the editor to be closed again.
                    while (IsPauseMenuActive())
                    {
                        await BaseScript.Delay(0);
                    }
                    // then fade in the screen.
                    DoScreenFadeIn(1);
                    Notify.Alert(GetString("Recording_EditorQuit"), true, true);
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
