using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
namespace HydraOS.Kernel
{

    public class HydraThemeSync
    {
        private readonly CoreWebView2 _web;

        public HydraThemeSync(CoreWebView2 web)
        {
            _web = web;

            // Escuta mudanças de tema no Windows
            SystemEvents.UserPreferenceChanged += async (s, e) =>
            {
                if (e.Category == UserPreferenceCategory.General)
                    await PushThemeToWeb();
            };
        }

        /// <summary>
        /// Obtém o tema atual do Windows.
        /// </summary>
        public static string GetSystemTheme()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                if (key == null)
                    return "light";

                int value = (int)key.GetValue("AppsUseLightTheme", 1);

                return value == 1 ? "light" : "dark";
            }
            catch
            {
                return "light";
            }
        }

        /// <summary>
        /// Envia o tema atual para o glass-site.
        /// </summary>
        public async Task PushThemeToWeb()
        {
            string theme = GetSystemTheme();

            string script = $"window.HydraTheme?.apply('{theme}');";
            await _web.ExecuteScriptAsync(script);
        }
    }
}
