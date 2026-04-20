using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace HydraOS.Kernel
{
    public class HydraShellAnimations
    {
        private readonly CoreWebView2 _web;

        public HydraShellAnimations(CoreWebView2 web)
        {
            _web = web;
        }

        /// <summary>
        /// Fade-in inicial do HydraShell (JS + CSS).
        /// </summary>
        public async Task FadeIn()
        {
            string script = "window.HydraFX?.fadeIn();";
            await _web.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Ativa o blur progressivo no glass-site.
        /// </summary>
        public async Task BlurIn()
        {
            string script = "window.HydraFX?.blurIn();";
            await _web.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Remove blur e volta ao estado nítido.
        /// </summary>
        public async Task BlurOut()
        {
            string script = "window.HydraFX?.blurOut();";
            await _web.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Transição suave entre temas ou modos.
        /// </summary>
        public async Task SmoothTransition()
        {
            string script = "window.HydraFX?.smoothTransition();";
            await _web.ExecuteScriptAsync(script);
        }
    }
}
