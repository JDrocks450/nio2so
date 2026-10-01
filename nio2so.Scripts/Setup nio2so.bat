@echo off
:menu
cls
echo ============================
echo Setup nio2so for First Run Wizard
echo This will help you get your server started for the first time!
echo ============================
echo You will be taken through these steps:
echo 1. CHECKING HOSTS FILE
echo 2. TRUSTING THE HTTPS CERTIFICATE
echo 3. ENSURING The Sims Online Pre-Alpha HAS BEEN INSTALLED AND PATCHED
echo 4. RUNNING THE SERVER ONCE TO CHECK SETTINGS
echo ============================
pause

cls
echo ============================
echo 1. CHECKING HOSTS FILE
echo Ensure your HOSTS file has proper entries.
echo ============================
echo Your HOSTS file must have these entries:
echo(
echo    127.0.0.1 www.ea.com       # AuthLogin for TSO
echo  	127.0.0.1 xo.max.ad.ea.com # The Sims Online: Pre-Alpha
echo(
echo ============================ 
echo Please take this time to add those entries to the hosts file, usually found at this file path:
echo "C:\Windows\System32\drivers\etc\hosts"
choice /c YN /m "Open Explorer to this path now?"
if errorlevel 1 (
	explorer "C:\Windows\System32\drivers\etc"
	rem Yes action
)

cls
echo ============================
echo 1. CHECKING HOSTS FILE
echo Ensure your HOSTS file has proper entries.
echo ============================
echo Your HOSTS file currently looks like this:
echo ============================
echo(
type "C:\Windows\System32\drivers\etc\hosts"
echo(
echo ============================
echo Your file should have the aforementioned entries. Otherwise, close the setup program and add them now.
pause
