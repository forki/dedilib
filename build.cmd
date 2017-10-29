@echo off
%~dp0.paket\paket.bootstrapper.exe || (pause && exit /b 1)
%~dp0.paket\paket restore || (pause && exit /b 1)

msbuild.exe %~dp0DediLib.sln /p:OutDir=%~dp0build /p:BuildInParallel=true /p:Configuration=Release || (pause && exit /b 1)
