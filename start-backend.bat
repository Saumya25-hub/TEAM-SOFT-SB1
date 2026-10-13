@echo off
title Simple Billing - Backend API Server
echo ========================================================
echo   Starting Simple Billing Multi-Tenant Backend Server
echo ========================================================
cd /d "%~dp0backend"
npm run dev
pause
