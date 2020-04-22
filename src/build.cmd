rem windows only
rem NSP v1.3
@ECHO off

set Ver=v1.3_alpha
set BuildPath=%~dp0../build

set nsp_client_path=%BuildPath%/nspclient_%Ver%
set nsp_server_path=%BuildPath%/nspserver_%Ver%

set nsp_client_scd_linux_path=%BuildPath%/nspclient_scd_linux_%Ver%
set nsp_client_scd_win_path=%BuildPath%/nspclient_scd_win_%Ver%
set nsp_client_scd_osx_path=%BuildPath%/nspclient_scd_osx_%Ver%
set nsp_client_scd_linux_arm_path=%BuildPath%/nspclient_scd_linux_arm_%Ver%
set nsp_client_scd_win_arm_path=%BuildPath%/nspclient_scd_win_arm_%Ver%

set nsp_server_scd_linux_path=%BuildPath%/nspserver_scd_linux_%Ver%
set nsp_server_scd_win_path=%BuildPath%/nspserver_scd_win_%Ver%
set nsp_server_scd_osx_path=%BuildPath%/nspserver_scd_osx_%Ver%
set nsp_server_scd_linux_arm_path=%BuildPath%/nspserver_scd_linux_arm_%Ver%
set nsp_server_scd_win_arm_path=%BuildPath%/nspserver_scd_win_arm_%Ver%

set nsp_client_winfform_path=%BuildPath%/nspclient_winform_%Ver%

rem del %~dp0/../build/*.*
rem NSPClient
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -c release -o %nsp_client_path%

rem NSPServer
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c release -o %nsp_server_path%

rem NSPClient_SCD
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_client_scd_linux_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_client_scd_win_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r osx-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_client_scd_osx_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r linux-arm -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_client_scd_linux_arm_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r win-arm -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_client_scd_win_arm_path%

rem NSPServer_SCD
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_server_scd_linux_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_server_scd_win_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r osx-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_server_scd_osx_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r linux-arm -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_server_scd_linux_arm_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r win-arm -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o %nsp_server_scd_win_arm_path%

rem NSPWinform
MSBuild .\NSmartProxyWinform\NSmartProxyWinform.csproj /t:build /p:OutDir=%nsp_client_winfform_path%
powershell del %nsp_client_winfform_path%/*.pdb
powershell del %nsp_client_winfform_path%/*.xml

rem ilmerge
rem ruined :<

rem compress
powershell Compress-Archive -Path '%nsp_client_path%/*' -DestinationPath '%nsp_client_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_server_path%/*' -DestinationPath '%nsp_server_path%.zip' -Force 

powershell Compress-Archive -Path '%nsp_client_scd_linux_path%/*' -DestinationPath '%nsp_client_scd_linux_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_client_scd_win_path%/*' -DestinationPath '%nsp_client_scd_win_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_client_scd_osx_path%/*' -DestinationPath '%nsp_client_scd_osx_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_client_scd_linux_arm_path%/*' -DestinationPath '%nsp_client_scd_linux_arm_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_client_scd_win_arm_path%/*' -DestinationPath '%nsp_client_scd_win_arm_path%.zip' -Force 

powershell Compress-Archive -Path '%nsp_server_scd_linux_path%/*' -DestinationPath '%nsp_server_scd_linux_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_server_scd_win_path%/*' -DestinationPath '%nsp_server_scd_win_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_server_scd_osx_path%/*' -DestinationPath '%nsp_server_scd_osx_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_server_scd_linux_arm_path%/*' -DestinationPath '%nsp_server_scd_linux_arm_path%.zip' -Force 
powershell Compress-Archive -Path '%nsp_server_scd_win_arm_path%/*' -DestinationPath '%nsp_server_scd_win_arm_path%.zip' -Force 


powershell Compress-Archive -Path '%nsp_client_winfform_path%/*' -DestinationPath '%BuildPath%/nspclient_winform_%Ver%.zip' -Force 

powershell explorer %~dp0..\build
pause