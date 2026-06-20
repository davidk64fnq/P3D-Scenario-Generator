@echo off
title Launching AI Coding Environment...

echo [1/3] Starting LM Studio Background Server...
call lms server start

echo.
echo [2/3] Pre-loading Qwen into VRAM (16k Context)...
call lms load qwen2.5-coder-7b-instruct --context-length 16384 --gpu max

echo.
echo [3/3] Opening Project Shell and Initializing Aider...
timeout /t 2 >nul

:: Launched as a single continuous execution block to avoid shell syntax conflicts
start powershell.exe -NoExit -Command "cd 'C:\Users\david\source\repos\davidk64fnq\P3D-Scenario-Generator'; $env:LM_STUDIO_API_BASE='http://localhost:1234/v1'; $env:LM_STUDIO_API_KEY='dummy-api-key'; Write-Host '--- Environment Ready. Initializing Aider ---' -ForegroundColor Cyan; aider --model lm_studio/qwen2.5-coder-7b-instruct --no-show-model-warnings" "C:\Users\david\source\repos\davidk64fnq\P3D-Scenario-Generator"

exit