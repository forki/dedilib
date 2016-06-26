@echo off
%~dp0.paket\paket install --redirects || (pause && exit /b 1)
