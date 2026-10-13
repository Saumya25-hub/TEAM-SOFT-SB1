@echo off
title Simple Billing - Desktop Application
echo ========================================================
echo   Starting Simple Billing Desktop App (Electron+React)
echo ========================================================
cd /d "%~dp0desktop"
npm run electron:dev
pause
