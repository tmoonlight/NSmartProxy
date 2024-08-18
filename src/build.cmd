rem windows only
rem NSP v1.4.1
@ECHO on

set Ver=v1.4.1
set BuildPath=%~dp0../build

set nsp_client_path=%BuildPath%/nspclient_unity_%Ver%
set nsp_server_path=%BuildPath%/nspserver_unity_%Ver%

set nsp_client_scd_linux_path=%BuildPath%/nspclient_scd_linux_%Ver%
set nsp_client_scd_win_path=%BuildPath%/nspclient_scd_win_x86_%Ver%
set nsp_client_scd_osx_path=%BuildPath%/nspclient_scd_osx_x86_%Ver%
set nsp_client_scd_osx_arm_path=%BuildPath%/nspclient_scd_osx_arm64_%Ver%
set nsp_client_scd_linux_arm_path=%BuildPath%/nspclient_scd_linux_arm64_%Ver%
set nsp_client_scd_win_arm_path=%BuildPath%/nspclient_scd_win_arm64_%Ver%

set nsp_server_scd_linux_path=%BuildPath%/nspserver_scd_linux_%Ver%
set nsp_server_scd_win_path=%BuildPath%/nspserver_scd_win_%Ver%
set nsp_server_scd_osx_path=%BuildPath%/nspserver_scd_osx_%Ver%
set nsp_server_scd_linux_arm_path=%BuildPath%/nspserver_scd_linux_arm_%Ver%
set nsp_server_scd_win_arm_path=%BuildPath%/nspserver_scd_win_arm_%Ver%

set nsp_client_winfform_path=%BuildPath%/nspclient_winform_%Ver%



rem NSPClient
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -c release -o %nsp_client_path% /p:DebugType=None /p:PublishTrimmed=false

rem NSPServer
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c release -o %nsp_server_path% /p:DebugType=None /p:PublishTrimmed=false

rem NSPClient_SCD
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r linux-x64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_client_scd_linux_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r win-x64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_client_scd_win_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r osx-x64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_client_scd_osx_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r osx-arm64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_client_scd_osx_arm_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r linux-arm -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_client_scd_linux_arm_path%
dotnet publish .\NSmartProxyClient\NSmartProxyClient.csproj -r win-arm64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_client_scd_win_arm_path%

rem NSPServer_SCD
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r linux-x64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_server_scd_linux_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r win-x64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_server_scd_win_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r osx-x64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_server_scd_osx_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r osx-arm64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_server_scd_osx_arm_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r linux-arm -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_server_scd_linux_arm_path%
dotnet publish .\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -r win-arm64 -c Release /p:PublishSingleFile=true /p:DebugType=None  -o %nsp_server_scd_win_arm_path%

rem NSPWinform
MSBuild .\NSmartProxyWinform\NSmartProxyWinform.csproj /t:build /p:OutDir=%nsp_client_winfform_path%
powershell del %nsp_client_winfform_path%\*.pdb
powershell del %nsp_client_winfform_path%\*.xml



rem ilmerge
rem ruined :<

rem compress
powershell Compress-Archive -Path '%nsp_client_path%/' -DestinationPath '%nsp_client_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_server_path%/' -DestinationPath '%nsp_server_path%.zip' -Force

powershell Compress-Archive -Path '%nsp_client_scd_linux_path%/' -DestinationPath '%nsp_client_scd_linux_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_client_scd_win_path%/' -DestinationPath '%nsp_client_scd_win_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_client_scd_osx_path%/' -DestinationPath '%nsp_client_scd_osx_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_client_scd_linux_arm_path%/' -DestinationPath '%nsp_client_scd_linux_arm_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_client_scd_win_arm_path%/*' -DestinationPath '%nsp_client_scd_win_arm_path%.zip' -Force

powershell Compress-Archive -Path '%nsp_server_scd_linux_path%/' -DestinationPath '%nsp_server_scd_linux_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_server_scd_win_path%/' -DestinationPath '%nsp_server_scd_win_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_server_scd_osx_path%/' -DestinationPath '%nsp_server_scd_osx_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_server_scd_linux_arm_path%/' -DestinationPath '%nsp_server_scd_linux_arm_path%.zip' -Force
powershell Compress-Archive -Path '%nsp_server_scd_win_arm_path%/*' -DestinationPath '%nsp_server_scd_win_arm_path%.zip' -Force

powershell Compress-Archive -Path '%nsp_client_winfform_path%/*' -DestinationPath '%BuildPath%/nspclient_winform_%Ver%.zip' -Force

powershell explorer %~dp0…\build
pause