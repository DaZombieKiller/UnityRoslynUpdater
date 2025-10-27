@echo off
dotnet publish -c Release -r win-x64 -p:DebugType=none
pause