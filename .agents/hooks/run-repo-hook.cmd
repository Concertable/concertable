@echo off
setlocal
for /f "delims=" %%R in ('git rev-parse --show-toplevel 2^>nul') do set "REPO_ROOT=%%R"
if not defined REPO_ROOT exit /b 1
python -B "%REPO_ROOT%\.agents\hooks\%~1"
exit /b %errorlevel%
