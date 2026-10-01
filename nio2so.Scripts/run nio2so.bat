@echo off
cd "./TSO HTTPS"
start "" "nio2so.TSOHTTPS.exe" --urls https://localhost:443;http://localhost:80
cd ".."
cd "./DataService API"
start "" "nio2so.DataService.API.exe" --urls https://localhost:7071;http://localhost:5231
cd ".."
call  "./Voltron Server/nio2so.TSOTCP.Voltron.Server.exe"
pause