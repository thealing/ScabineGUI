@echo off
dotnet publish ChessPanel.sln -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish\win-x64
dotnet publish ChessPanel.sln -c Release -r win-x86 --self-contained false -p:PublishSingleFile=true -o publish\win-x86
pause
