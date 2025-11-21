using System.Globalization;
using System.Resources;

namespace vMenuClient
{
    /// <summary>
    /// Localization helper class for accessing translated strings.
    /// </summary>
    public static class Localization
    {
        private static ResourceManager resourceManager;
        private static CultureInfo currentCulture = new CultureInfo("ko-KR");

        static Localization()
        {
            // Initialize the resource manager to use Korean resources
            resourceManager = new ResourceManager("vMenuClient.Properties.Resources.ko", typeof(Localization).Assembly);
        }

        /// <summary>
        /// Gets a localized string by key.
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public static string GetString(string key)
        {
            try
            {
                var result = resourceManager.GetString(key, currentCulture);
                return result ?? key; // Return key if translation not found
            }
            catch
            {
                return key; // Fallback to key if error occurs
            }
        }

        /// <summary>
        /// Gets a localized string with format parameters.
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        public static string GetString(string key, params object[] args)
        {
            try
            {
                var format = GetString(key);
                return string.Format(format, args);
            }
            catch
            {
                return key;
            }
        }
    }
}
