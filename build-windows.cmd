@echo off
dotnet publish -c Release -r win-x64 --no-self-contained -p:DebugType=none -p:PublishSingleFile=true
