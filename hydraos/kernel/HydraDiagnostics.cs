using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace HydraOS.Kernel
{

    public class HydraDiagnostics
    {
        private readonly CoreWebView2 _web;

        public HydraDiagnostics(CoreWebView2 web)
        {
            _web = web;
        }

        /// <summary>
        /// Envia uma mensagem de debug para o painel no glass-site.
        /// </summary>
        public async Task Log(string message)
        {
            string safe = message.Replace("'", "\\'");
            string script = $"window.HydraDiagnostics?.push('{safe}');";
            await _web.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Envia um objeto JSON para debug.
        /// </summary>
        public async Task LogObject(object obj)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(obj);
            string script = $"window.HydraDiagnostics?.push(JSON.stringify({json}));";
            await _web.ExecuteScriptAsync(script);
        }
    }
}
