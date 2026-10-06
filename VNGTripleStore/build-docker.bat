@echo off

echo Rebuilding and starting Docker container...
REM docker compose down
docker compose build --no-cache
REM docker compose up