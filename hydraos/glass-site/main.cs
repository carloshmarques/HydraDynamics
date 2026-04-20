using System;
using System.Diagnostics;
using System.Windows.Forms; // Ou o namespace do teu projeto

public partial class MainApp : Form
{
    // Contadores de Performance do Windows
    private PerformanceCounter cpuCounter;
    private Timer systemTimer;

    public MainApp()
    {
        InitializeComponent();
        InitCounters();
        InitApp();
    }

    private void InitCounters()
    {
        // Inicializa o leitor de CPU do Windows
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        
        // Timer para atualizar a interface a cada 1 segundo (1000ms)
        systemTimer = new Timer();
        systemTimer.Interval = 1000;
        systemTimer.Tick += async (s, e) => await UpdateSystemStats();
    }

    async void InitApp()
    {
        // Espera a WebView estar pronta
        await webView.EnsureCoreWebView2Async();
        
        // Carrega o endereço do BrowserSync/Gulp
        webView.CoreWebView2.Navigate("http://localhost:3000");

        // Evento: Quando a página termina de carregar, verifica a performance
        webView.NavigationCompleted += (s, e) => {
            UpdatePerformanceMode();
            systemTimer.Start(); // Inicia o monitor de CPU
        };
    }

    /// <summary>
    /// Deteta RAM do Sistema e injeta classe de performance no CSS
    /// </summary>
    public void UpdatePerformanceMode()
    {
        // Obtém a RAM total instalada (em GB)
        var gcMemoryInfo = GC.GetGCMemoryInfo();
        long totalMemoryGB = gcMemoryInfo.TotalAvailableMemoryBytes / 1024 / 1024 / 1024;

        if (totalMemoryGB < 4)
        {
            string script = "document.documentElement.classList.add('low-memory');";
            webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }

    /// <summary>
    /// Envia dados reais da CPU para o monitor de vidro (JS/SCSS)
    /// </summary>
    private async System.Threading.Tasks.Task UpdateSystemStats()
    {
        if (webView.CoreWebView2 == null) return;

        float cpuUsage = cpuCounter.NextValue();
        int roundedCpu = (int)Math.Round(cpuUsage);

        // Script que move a barra e o texto que criámos no index.html
        string script = $@"
            if(document.getElementById('cpu-bar')) {{
                document.getElementById('cpu-bar').style.width = '{roundedCpu}%';
                document.getElementById('cpu-value').innerText = '{roundedCpu}%';
            }}";

        await webView.CoreWebView2.ExecuteScriptAsync(script);
    }
}
