using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace HydraOS.Kernel
{
    public class HydraNotifications
    {
        private readonly CoreWebView2 _web;

        public HydraNotifications(CoreWebView2 web)
        {
            _web = web;
        }

        public async Task Push(string title, string message, string type = "info")
        {
            string safeTitle = title.Replace("'", "\\'");
            string safeMsg = message.Replace("'", "\\'");

            string script = $@"
                window.HydraNotify?.push('{safeTitle}', '{safeMsg}', '{type}');
            ";

            await _web.ExecuteScriptAsync(script);
        }
    }
}
