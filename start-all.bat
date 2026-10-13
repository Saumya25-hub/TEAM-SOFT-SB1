@echo off
title Simple Billing - Master Launcher
echo ========================================================
echo   Launching Simple Billing Suite (Backend + Desktop App)
echo ========================================================
start "Simple Billing Backend" cmd /c "%~dp0start-backend.bat"
timeout /t 3 /nobreak >nul
start "Simple Billing Desktop" cmd /c "%~dp0start-desktop.bat"
exit
