@echo off
title HydraLife CORE Launcher - sporting edition
color 0A

echo ================================================
echo        HYDRALIFE CORE - LAUNCHER (BAT)
echo     PowerShell 7 + ExecutionPolicy Bypass
echo ================================================
echo.

REM --- Verificar se está em modo administrador ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] A elevar privilegios de administrador...
    powershell -NoProfile -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo [OK] Permissoes elevadas confirmadas.
echo.

REM --- Detectar PowerShell 7 ---
where pwsh >nul 2>&1
if %errorlevel% equ 0 (
    echo [>] PowerShell 7 encontrado. A iniciar HydraLife_CORE.ps1...
    pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0HydraLife_CORE.ps1"
) else (
    echo [!] PowerShell 7 nao encontrado. A usar PowerShell 5...
    powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0HydraLife_CORE.ps1"
)

echo.
echo ================================================
echo        HYDRALIFE CORE - SESSAO TERMINADA
echo ================================================
pause
exit
