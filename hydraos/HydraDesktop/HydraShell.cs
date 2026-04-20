using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

using HydraOS.Kernel;

namespace HydraDesktop
{
    public partial class HydraShell : Form
    {
        private WebView2 webView;
        private System.Windows.Forms.Timer systemTimer;

        private HydraSystemMonitor monitor;
        private HydraBridge bridge;
        private HydraDiagnostics diagnostics;
        private HydraEvents events;
        private HydraThemeSync themeSync;
        private HydraShellAnimations fx;
        private HydraNotifications notify;

        public HydraShell()
        {
            InitializeComponent();
            InitWebView();
            InitCore();
            InitApp();
        }

        private async void InitWebView()
        {
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(webView);

            await webView.EnsureCoreWebView2Async();
        }

        private void InitCore()
        {
            monitor = new HydraSystemMonitor();

            systemTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

            systemTimer.Tick += async (s, e) =>
            {
                if (webView.CoreWebView2 != null)
                    await monitor.PushCpuToWeb(webView.CoreWebView2);
            };
        }

        private async void InitApp()
        {
            if (webView.CoreWebView2 == null)
            {
                await webView.EnsureCoreWebView2Async();
            }

            bridge = new HydraBridge(webView.CoreWebView2);
            diagnostics = new HydraDiagnostics(webView.CoreWebView2);
            events = new HydraEvents(webView.CoreWebView2);
            themeSync = new HydraThemeSync(webView.CoreWebView2);
            fx = new HydraShellAnimations(webView.CoreWebView2);
            notify = new HydraNotifications(webView.CoreWebView2);

            await diagnostics.Log("HydraShell iniciado.");
            await diagnostics.LogObject(new { cpu = 12, ram = 8 });
            await diagnostics.Log("HydraEvents inicializado.");
            await diagnostics.Log("HydraThemeSync inicializado.");

            await themeSync.PushThemeToWeb();

            await notify.Push("HydraShell", "Sistema iniciado com sucesso.", "info");

            webView.NavigationCompleted += async (s, e) =>
            {
                string mode = HydraHostMode.Detect();
                HydraInjector.InjectClass(webView.CoreWebView2, mode);

                await notify.Push("HydraOS", "Interface carregada.", "success");

                await fx.FadeIn();
                await fx.BlurIn();

                systemTimer.Start();
            };

            webView.CoreWebView2.Navigate("http://localhost:3000");
        }
    }
}
