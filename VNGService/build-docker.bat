@echo off
echo Publishing...
dotnet publish VNGService/VNGService.csproj -c Release -r linux-x64 --self-contained false -o VNGService/bin/Release/net9.0/publish

echo Rebuilding and starting Docker container...
REM docker compose down
docker compose build --no-cache
REM docker compose up