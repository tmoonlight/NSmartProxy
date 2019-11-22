@echo off
echo *** NSP TEST ***

set buildPath=%~dp0\build
set testPath=%buildPath%\netcoreapp3.0
set nspClientPath=%buildPath%\nspclient\netcoreapp3.0
set nspServerPath=%buildPath%\nspserver\netcoreapp3.0

REM BUILD
MSBuild .\TestBed.sln /t:build
dotnet build ..\src\NSmartProxyClient\NSmartProxyClient.csproj
dotnet build ..\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj


rem run nsmartproxy
start "" cmd /c "%nspServerPath%\NSmartProxy.ServerHost.exe"
start "" cmd /c "%nspClientPath%\NSmartProxyClient.exe"

rem Wait them ready...
powershell Start-Sleep -Seconds 3

rem appsettings
copy "%buildPath%\_appsettings_server.txt" "%nspServerPath%\appsettings.json" /y
copy "%buildPath%\_appsettings_client.txt" "%nspClientPath%\appsettings.json" /y

rem start server
start "" cmd /c "%testPath%/TcpServer.exe"
%testPath%/TcpClient.exe

rem start client
start "" cmd /c "dotnet %testPath%/UdpServer.dll"
dotnet %testPath%/UdpClient.dll

echo ===============================================
echo TEST ACCOMPLISHeD
pause 
rem test service1 TCP
rem test service2 UDP
rem test service3 HTTP1
rem test service4 HTTP2
