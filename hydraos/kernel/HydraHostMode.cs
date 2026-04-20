using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace HydraOS.Kernel
{
    public static class HydraHostMode
    {

    public static string Detect()
    {
        var gc = GC.GetGCMemoryInfo();
        long totalGB = gc.TotalAvailableMemoryBytes / 1024 / 1024 / 1024;

        if (totalGB < 4)
            return "low-memory";

        if (totalGB < 8)
            return "balanced";

        return "high-performance";
    }
}
}
