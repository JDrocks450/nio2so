@echo off
:menu
cls
echo ============================
echo Adjust Certificates - nio2so
echo Please Note: You must have .NET 9.0 or later installed.
echo ============================
echo 1. Option 1: Trust Developer Certificate - needed to host nio2so locally.
echo 2. Option 2: Untrust Developer Certificate - to undo adding this certificate.
echo 3. Exit
echo ============================
set /p choice=Choose an option:

if "%choice%"=="1" goto option1
if "%choice%"=="2" goto option2
if "%choice%"=="3" goto exit
echo Invalid choice. Please try again
timeout /t 2 >nul
goto menu

:option1
cls
echo Adding the certificate using .NET ...
call dotnet dev-certs https --trust
timeout /t 2 >nul
goto exit

:option2
cls
echo Removing the certificate using .NET ...
call dotnet dev-certs https --clean
timeout /t 2 >nul
goto exit

:exit
cls
echo Exiting...
timeout /t 2 >nul
exit