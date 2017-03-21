@echo off
%~dp0.paket\paket.bootstrapper.exe || (pause && exit /b 1)
%~dp0.paket\paket restore || (pause && exit /b 1)

%~dp0packages\FAKE\tools\FAKE.exe build.fsx %* || (pause && exit /b 1)
