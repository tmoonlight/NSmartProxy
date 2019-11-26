rem windows only
rem NSP v1.1
@ECHO off

set Ver=v1.2_final4
set BuildPath=%~dp0../build

set nsp_server_path=%BuildPath%/nspclient_%Ver%
set nsp_client_path=%BuildPath%/nspserver_%Ver%
set nsp_client_winfform_path=%BuildPath%/nspclient_winform_%Ver%

rem del %~dp0/../build/*.*
rem NSPClient
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -c release -o %nsp_server_path%

rem NSPServer
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c release -o %nsp_client_path%

rem NSPWinform
MSBuild .\NSmartProxyWinform\NSmartProxyWinform.csproj /t:build /p:OutDir=%nsp_client_winfform_path%
powershell del %nsp_client_winfform_path%/*.pdb
powershell del %nsp_client_winfform_path%/*.xml

rem ilmerge
rem ruined :<

rem compress
powershell Compress-Archive -Path '%nsp_server_path%/*' -DestinationPath '%BuildPath%/nspclient_%Ver%.zip' -Force 
powershell Compress-Archive -Path '%nsp_client_path%/*' -DestinationPath '%BuildPath%/nspserver_%Ver%.zip' -Force 
powershell Compress-Archive -Path '%nsp_client_winfform_path%/*' -DestinationPath '%BuildPath%/nspclient_winform_%Ver%.zip' -Force 

powershell explorer %~dp0..\build
pause