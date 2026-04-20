using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace HydraOS.Kernel;

public class HydraSystemMonitor
{
    private readonly PerformanceCounter cpuCounter;

    public HydraSystemMonitor()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    }

    public async Task PushCpuToWeb(CoreWebView2 web)
    {
        float cpu = cpuCounter.NextValue();
        int rounded = (int)Math.Round(cpu);

        string script = $@"
            if (document.getElementById('cpu-bar')) {{
                document.getElementById('cpu-bar').style.width = '{rounded}%';
            }}
            if (document.getElementById('cpu-value')) {{
                document.getElementById('cpu-value').innerText = '{rounded}%';
            }}";

        await web.ExecuteScriptAsync(script);
    }
}
