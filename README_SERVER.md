<img src="https://github.com/tmoonlight/NSmartProxy/blob/master/NSmartProxyNew.png">

# NSmartProxy Server

Here is the installation method of NSmartProxy server (Linux, windows, MacOS are compatible)<br />

## Startup preparation
* First of all, you need a server with a separate IP, the following installation process is performed on this machine:
#### Linux/Windows/MacOS
1. Install [.NET Core Environment](https://dotnet.microsoft.com/download)<br />
2. Download the latest version of [NSmartProxy](https://github.com/tmoonlight/NSmartProxy/releases)

#### Docker
* You can run the nspserver directly without having to install the runtime. Four sets of ports are required to run the docker image: configuration port, reverse connection port, API service port and consumer port:
```
sudo docker pull tmoonlight/nspserver
sudo docker run --name mynspserver -dit -p 7842:7842 -p 7841:7841 -p 12309:12309 -p 20000-20050 tmoonlight/nspserver
```

## Instructions
1. Unzip the package of NSmartProxy server.
2. Open the appsettings.json file in the installation directory, set the reverse connection port and configure the service port:<br />
```
{
  "ReversePort": 7842, //Reverse connection port
  "ConfigPort": 7841, //Configure the service port
  "WebAPIPort": 12309 //API service port
}
```
<br />
3. Run NSmartProxy Server<br />


* Linux/MacOS:
Change directory to the installation directory ,then execute the following command:
```
sudo dotnet NSmartProxy.ServerHost.dll
```
* Windows:
Press Windows+R to open the “Run” box. Type “cmd” into the box and then press Ctrl+Shift+Enter to run the command as an administrator.
Change directory to the installation directory ,then execute the following command:
```
dotnet NSmartProxy.ServerHost.dll
```


In the next step,you can log in to http://youraddress:12309 and enter the web terminal. The default user password is admin/admin.

<img src="https://github.com/tmoonlight/100lines/raw/master/6.nspserverrunnning_1.gif" />

And enter the server to perform various management operations.

<img src="https://github.com/tmoonlight/100lines/raw/master/6.nspserverrunnning_2.gif" />
