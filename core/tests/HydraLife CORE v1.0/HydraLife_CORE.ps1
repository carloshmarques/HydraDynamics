# ============================================
# HYDRALIFE CORE v1.2 - SISTEMA DE DIAGNÓSTICO (LINUX HYBRID)
# Base: v1.1 + Nova HydraStep com barra + spinner híbrido
# Motor híbrido: PowerShell 7 + PowerShell 5.1
# ============================================

Clear-Host
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "          HYDRALIFE CORE - INICIAR          " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ================================
# NOVA HYDRASTEP v1.2 (LINUX HYBRID)
# ================================
function HydraStep {
    param([string]$Texto)

    # Detectar suporte Unicode
    $unicode = $false
    try {
        if ([Console]::OutputEncoding.WebName -like "*utf*") { $unicode = $true }
    } catch {}

    # Frames do spinner + caracteres da barra
    if ($unicode) {
        $frames = @("⠋","⠙","⠹","⠸","⠼","⠴","⠦","⠧","⠇","⠏")
        $full   = "█"
        $empty  = "░"
    } else {
        $frames = @("|","/","-","\")
        $full   = "#"
        $empty  = "-"
    }

    $largura = 24
    $percent = 0
    $step    = 4  # velocidade da barra

    Write-Host ""
    Write-Host ">>> $Texto" -ForegroundColor Cyan

    while ($percent -lt 100) {
        $filledCount = [int]($percent / (100 / $largura))
        if ($filledCount -gt $largura) { $filledCount = $largura }
        $emptyCount  = $largura - $filledCount

        $filled = $full * $filledCount
        $emptyB = $empty * $emptyCount

        $frameIndex = [int]($percent / $step)
        $frame      = $frames[$frameIndex % $frames.Count]

        $linha = "[${filled}${emptyB}] ${percent}%  ${frame}"

        Write-Host "`r$linha" -NoNewline -ForegroundColor Yellow
        Start-Sleep -Milliseconds 80

        $percent += $step
    }

    Write-Host "`r[$($full * $largura)] 100%  OK " -ForegroundColor Green
}

# ================================
# Função que executa comandos no PowerShell 5.1
# ================================
function Invoke-PS51 {
    param([string]$Command)

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "powershell.exe"
    $psi.Arguments = "-NoLogo -NoProfile -Command $Command"
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.UseShellExecute        = $false
    $psi.CreateNoWindow         = $true

    $p = [System.Diagnostics.Process]::Start($psi)
    $out = $p.StandardOutput.ReadToEnd()
    $err = $p.StandardError.ReadToEnd()
    $p.WaitForExit()

    return [PSCustomObject]@{
        Output = $out
        Error  = $err
    }
}

# ================================
# 1. SISTEMA - INFO GERAL
# ================================
HydraStep "A recolher informações do sistema..."

$OS  = Get-CimInstance Win32_OperatingSystem
$CPU = Get-CimInstance Win32_Processor
$RAM = [math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)

Write-Host ""
Write-Host "=== SISTEMA ===" -ForegroundColor Cyan
Write-Host "Windows: $($OS.Caption) ($($OS.OSArchitecture))"
Write-Host "CPU: $($CPU.Name)"
Write-Host "RAM Total: $RAM GB"
Write-Host ""

# ================================
# 2. LOGS DO WINDOWS
# ================================
HydraStep "A analisar logs do Windows..."

$CBSLog  = "$env:windir\Logs\CBS\CBS.log"
$DISMLog = "$env:windir\Logs\DISM\dism.log"

Write-Host ""
Write-Host "=== LOGS ===" -ForegroundColor Cyan

if (Test-Path $CBSLog) {
    Write-Host "CBS.log encontrado."
} else {
    Write-Host "CBS.log não encontrado."
}

if (Test-Path $DISMLog) {
    Write-Host "DISM.log encontrado."
} else {
    Write-Host "DISM.log não encontrado."
}

Write-Host ""

# ================================
# 3. DRIVERS - ESTADO E PROBLEMAS
# ================================
HydraStep "A verificar drivers instalados..."

$Drivers = Get-WmiObject Win32_PnPSignedDriver | Select-Object DeviceName, DriverVersion, Manufacturer

Write-Host ""
Write-Host "=== DRIVERS ===" -ForegroundColor Cyan
Write-Host "Total de drivers instalados: $($Drivers.Count)"
Write-Host ""

$BadDrivers = Get-WmiObject Win32_PnPEntity | Where-Object { $_.ConfigManagerErrorCode -ne 0 }

if ($BadDrivers) {
    Write-Host "Drivers com problemas:" -ForegroundColor Yellow
    $BadDrivers | Select-Object Name, ConfigManagerErrorCode | Format-Table -AutoSize
} else {
    Write-Host "Nenhum driver com erro encontrado." -ForegroundColor Green
}

Write-Host ""

# ================================
# 4. SERVIÇOS IMPORTANTES
# ================================
HydraStep "A verificar serviços essenciais..."

$Servicos = @(
    "wuauserv",   # Windows Update
    "bits",       # Background Intelligent Transfer
    "WinDefend",  # Windows Defender
    "EventLog",   # Logs do Windows
    "Dhcp",       # DHCP
    "Dnscache"    # DNS
)

Write-Host ""
Write-Host "=== SERVIÇOS ===" -ForegroundColor Cyan

foreach ($svc in $Servicos) {
    $estado = Get-Service -Name $svc -ErrorAction SilentlyContinue
    if ($estado) {
        Write-Host "$svc : $($estado.Status)"
    } else {
        Write-Host "$svc : Não encontrado" -ForegroundColor Yellow
    }
}

Write-Host ""

# ================================
# 5. LIMPEZA TEMPORÁRIA
# ================================
HydraStep "A limpar ficheiros temporários..."

$TempPaths = @(
    "$env:TEMP\*",
    "$env:windir\Temp\*"
)

foreach ($p in $TempPaths) {
    try {
        Remove-Item $p -Recurse -Force -ErrorAction SilentlyContinue
    } catch {}
}

Write-Host ""
Write-Host "Limpeza concluída." -ForegroundColor Green
Write-Host ""

# ================================
# 6. SEGURANÇA - WINDOWS DEFENDER (PS5.1)
# ================================
HydraStep "A verificar estado do Windows Defender (motor híbrido)..."

Write-Host ""
Write-Host "=== SEGURANÇA ===" -ForegroundColor Cyan

$def = Invoke-PS51 -Command "Get-MpComputerStatus"

if ($def.Error) {
    Write-Host "Erro (PS5.1):" -ForegroundColor Yellow
    Write-Host $def.Error
} else {
    Write-Host $def.Output
}
Write-Host ""

# ================================
# 7. REDE - DNS, IP, LAG (PS5.1 + PS7)
# ================================
HydraStep "A recolher informações de rede (motor híbrido)..."

Write-Host ""
Write-Host "=== REDE ===" -ForegroundColor Cyan

$net = Invoke-PS51 -Command "Get-NetIPAddress; Get-DnsClientServerAddress"

if ($net.Error) {
    Write-Host "Erro (PS5.1):" -ForegroundColor Yellow
    Write-Host $net.Error
} else {
    Write-Host $net.Output
}
Write-Host ""

HydraStep "A testar latência (ping Google)..."

$Ping = Test-Connection -Count 3 -ComputerName 8.8.8.8 -ErrorAction SilentlyContinue

if ($Ping) {
    $Media = ($Ping | Measure-Object -Property ResponseTime -Average).Average
    Write-Host "Latência média: $([math]::Round($Media, 2)) ms"
} else {
    Write-Host "Falha no teste de ping." -ForegroundColor Yellow
}

Write-Host ""

# ================================
# 8. DISCO - SAÚDE E ERROS (PS5.1)
# ================================
HydraStep "A analisar discos (motor híbrido)..."

Write-Host ""
Write-Host "=== DISCO ===" -ForegroundColor Cyan

$disk = Invoke-PS51 -Command "Get-PhysicalDisk"

if ($disk.Error) {
    Write-Host "Erro (PS5.1):" -ForegroundColor Yellow
    Write-Host $disk.Error
} else {
    Write-Host $disk.Output
}
Write-Host ""

# ================================
# 9. INTEGRIDADE DO SISTEMA
# ================================
HydraStep "A verificar integridade do Windows (SFC)..."

Write-Host ""
Write-Host "=== SFC (OUTPUT COMPLETO) ===" -ForegroundColor Cyan

Start-Process -FilePath "sfc.exe" `
    -ArgumentList "/scannow" `
    -NoNewWindow -Wait

Write-Host ""

HydraStep "A verificar componentes do Windows (DISM)..."

Write-Host ""
Write-Host "=== DISM (OUTPUT COMPLETO E EM TEMPO REAL) ===" -ForegroundColor Cyan

Start-Process -FilePath "dism.exe" `
    -ArgumentList "/online","/cleanup-image","/scanhealth" `
    -NoNewWindow -Wait

Write-Host ""

# ================================
# 10. MODO GAMING (OPCIONAL)
# ================================
HydraStep "A preparar modo Gaming (opcional)..."

Write-Host ""
Write-Host "=== MODO GAMING ===" -ForegroundColor Cyan
Write-Host "1. Ativar modo Gaming"
Write-Host "2. Desativar modo Gaming"
Write-Host "3. Ignorar"
Write-Host ""

$gaming = Read-Host "Escolha uma opção"

switch ($gaming) {
    "1" {
        HydraStep "A aplicar otimizações Gaming..."
        powercfg -setactive SCHEME_MIN
        Write-Host "Modo Gaming ativado." -ForegroundColor Green
    }
    "2" {
        HydraStep "A restaurar plano equilibrado..."
        powercfg -setactive SCHEME_BALANCED
        Write-Host "Modo Gaming desativado." -ForegroundColor Yellow
    }
    default {
        Write-Host "Modo Gaming ignorado." -ForegroundColor Cyan
    }
}

Write-Host ""

# ================================
# 11. REPARAÇÕES OPCIONAIS
# ================================
Write-Host "=== REPARAÇÕES ===" -ForegroundColor Cyan
Write-Host "1. Reparar Windows Update"
Write-Host "2. Reparar Componentes (DISM RestoreHealth)"
Write-Host "3. Reparar Store"
Write-Host "4. Ignorar"
Write-Host ""

$fix = Read-Host "Escolha uma opção"

switch ($fix) {
    "1" {
        HydraStep "A reparar Windows Update..."
        net stop wuauserv
        net stop bits
        Remove-Item "$env:windir\SoftwareDistribution" -Recurse -Force -ErrorAction SilentlyContinue
        net start wuauserv
        net start bits
        Write-Host "Windows Update reparado." -ForegroundColor Green
    }
    "2" {
        HydraStep "A reparar componentes..."
        dism /online /cleanup-image /restorehealth
    }
    "3" {
        HydraStep "A reparar Microsoft Store..."
        wsreset -i
    }
    default {
        Write-Host "Reparações ignoradas." -ForegroundColor Cyan
    }
}

Write-Host ""

# ================================
# 12. FINALIZAÇÃO
# ================================
HydraStep "A finalizar sessão..."

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "        HYDRALIFE CORE - FINALIZADO         " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Obrigado por usar o HydraLife CORE!" -ForegroundColor Green
Write-Host ""
Pause
