@echo off
echo 🚀 Setup Custom Domain untuk Windows

REM Check if running as Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ⚠️  Right-click and "Run as Administrator"
    pause
    exit /b 1
)

REM Backup hosts file
copy "C:\Windows\System32\drivers\etc\hosts" "C:\Windows\System32\drivers\etc\hosts.backup.%date:~-4,4%%date:~-7,2%%date:~-10,2%_%time:~0,2%%time:~3,2%%time:~6,2%"
echo ✅ Hosts file backed up

REM Check if custom domains already exist
findstr /C:"frontend.com" "C:\Windows\System32\drivers\etc\hosts" >nul
if errorlevel 1 (
    REM Add custom domains
    echo. >> "C:\Windows\System32\drivers\etc\hosts"
    echo # Custom Local Domains >> "C:\Windows\System32\drivers\etc\hosts"
    echo 127.0.0.1   frontend.com >> "C:\Windows\System32\drivers\etc\hosts"
    echo 127.0.0.1   api.frontend.com >> "C:\Windows\System32\drivers\etc\hosts"
    echo 127.0.0.1   admin.frontend.com >> "C:\Windows\System32\drivers\etc\hosts"
    echo ✅ Custom domains added to hosts file
) else (
    echo ⚠️  Custom domains already exist in hosts file
)

REM Clear DNS cache
ipconfig /flushdns
echo ✅ DNS cache cleared

REM Test DNS
echo 🧪 Testing DNS resolution...
nslookup frontend.com
nslookup api.frontend.com
nslookup admin.frontend.com

echo.
echo 🎉 Setup complete! You can now use:
echo    http://frontend.com    (Frontend)
echo    http://api.frontend.com   (Backend API)
echo    http://admin.frontend.com (Admin Panel)

pause