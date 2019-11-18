rem windows only
rem NSP v1.2
@ECHO off

set Ver=v1.2pre3
set BuildPath=%~dp0../build

set nsp_client_path=%BuildPath%/nspclient_%Ver%
set nsp_server_path=%BuildPath%/nspserver_%Ver%


set nsp_client_scd_win_path=%BuildPath%/nspclient_scd_win_%Ver%



set nsp_server_scd_win_path=%BuildPath%/nspserver_scd_win_%Ver%


set nsp_client_winfform_path=%BuildPath%/nspclient_winform_%Ver%

rem del %~dp0/../build/*.*
rem NSPClient
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -c release -o %nsp_client_path%

rem NSPServer
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c release -o %nsp_server_path%

rem NSPClient_SCD
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r win-x64 -c Release /p:PublishSingleFile=true -o %nsp_client_scd_win_path%


rem NSPServer_SCD
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r win-x64 -c Release /p:PublishSingleFile=true -o %nsp_server_scd_win_path%

rem NSPWinform
MSBuild .\NSmartProxyWinform\NSmartProxyWinform.csproj /t:build /p:OutDir=%nsp_client_winfform_path%
powershell del %nsp_client_winfform_path%/*.pdb
powershell del %nsp_client_winfform_path%/*.xml

rem ilmerge
rem ruined :<

rem compress

powershell explorer %~dp0..\build
pause