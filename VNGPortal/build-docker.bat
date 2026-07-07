@echo off
echo Publishing...
dotnet publish VNGPortal/VNGPortal.csproj -c Release -r linux-x64 --self-contained false -o VNGPortal/bin/Release/net9.0/publish

echo Rebuilding and starting Docker container...
REM docker compose down
docker compose build --no-cache
REM docker compose up