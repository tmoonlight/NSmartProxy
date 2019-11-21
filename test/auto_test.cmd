rem autotest
set testPath=%~dp0/build/netcoreapp3.0
set nspClientPath=%~dp0/build/nspclient/netcoreapp3.0
set nspServerPath=%~dp0/build/nspserver/netcoreapp3.0

rem start server
start "" cmd /c "%testPath%/TcpServer.exe"
%testPath%/TcpClient.exe

rem start client
start "" cmd /c "%testPath%/UdpServer.exe"
%testPath%/UdpClient.exe

rem test service1 TCP
rem test service2 UDP
rem test service3 HTTP1
rem test service4 HTTP2
