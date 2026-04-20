using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
namespace HydraOS.Kernel
{

public static class HydraInjector
{
    public static void InjectClass(CoreWebView2 web, string className)
    {
        string script = $"document.documentElement.classList.add('{className}');";
        web.ExecuteScriptAsync(script);
    }
}
}
