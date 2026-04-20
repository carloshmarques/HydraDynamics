using System;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace HydraOS.Kernel
{

    public class HydraEvents
    {
        private readonly CoreWebView2 _web;

        public HydraEvents(CoreWebView2 web)
        {
            _web = web;
            RegisterIncomingEvents();
        }

        /// <summary>
        /// Regista eventos vindos do JavaScript.
        /// </summary>
        private void RegisterIncomingEvents()
        {
            _web.WebMessageReceived += async (s, e) =>
            {
                string raw = e.TryGetWebMessageAsString();

                try
                {
                    var evt = JsonSerializer.Deserialize<HydraEvent>(raw);

                    if (evt == null)
                        return;

                    Console.WriteLine($"[HydraEvents] JS → C#: {evt.Name}");

                    switch (evt.Name)
                    {
                        case "ping":
                            await Emit("pong", new { time = DateTime.Now.ToString() });
                            break;

                        case "request-hostmode":
                            string mode = HydraHostMode.Detect();
                            await Emit("hostmode", new { mode });
                            break;

                        default:
                            Console.WriteLine($"[HydraEvents] Evento desconhecido: {evt.Name}");
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("[HydraEvents] Mensagem inválida recebida.");
                }
            };
        }

        /// <summary>
        /// Emite um evento do C# para o JavaScript.
        /// </summary>
        public async Task Emit(string name, object payload = null)
        {
            var evt = new
            {
                name,
                payload
            };

            string json = JsonSerializer.Serialize(evt);
            string script = $"window.HydraEvents?.receive({json});";

            await _web.ExecuteScriptAsync(script);
        }

        private class HydraEvent
        {
            public string Name { get; set; }
            public JsonElement Payload { get; set; }
        }
    }
}
