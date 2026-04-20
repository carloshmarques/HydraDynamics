using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
namespace HydraOS.Kernel
{

    public class HydraBridge
    {
        private readonly CoreWebView2 _web;

        public HydraBridge(CoreWebView2 web)
        {
            _web = web;
            RegisterHandlers();
        }

        /// <summary>
        /// Regista handlers para mensagens vindas do JavaScript.
        /// </summary>
        private void RegisterHandlers()
        {
            _web.WebMessageReceived += async (s, e) =>
            {
                string message = e.TryGetWebMessageAsString();

                switch (message)
                {
                    case "ping":
                        await SendToWeb("pong");
                        break;

                    case "request-system-info":
                        await SendSystemInfo();
                        break;

                    default:
                        Console.WriteLine($"[HydraBridge] Mensagem desconhecida: {message}");
                        break;
                }
            };
        }

        /// <summary>
        /// Envia uma string simples para o JavaScript.
        /// </summary>
        public async Task SendToWeb(string msg)
        {
            string script = $"window.HydraBridge?.receive('{msg}');";
            await _web.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Exemplo: envia dados do sistema para o JS.
        /// </summary>
        private async Task SendSystemInfo()
        {
            string os = Environment.OSVersion.ToString();
            int cores = Environment.ProcessorCount;

            string script = $@"
                window.HydraBridge?.receive(JSON.stringify({{
                    os: '{os}',
                    cores: {cores}
                }}));
            ";

            await _web.ExecuteScriptAsync(script);
        }
    }
}
