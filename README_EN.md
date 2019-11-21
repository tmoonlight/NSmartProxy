<img src="https://github.com/tmoonlight/NSmartProxy/blob/master/NSmartProxyNew.png">

[![GitHub release](https://img.shields.io/github/release/tmoonlight/NSmartProxy.svg?logoColor=%21%5BGitHub%20release%5D%28https%3A%2F%2Fimg.shields.io%2Fgithub%2Frelease%2Ftmoonlight%2FNSmartProxy.svg%29)](https://github.com/tmoonlight/NSmartProxy/releases)
[![GitHub](https://img.shields.io/github/license/tmoonlight/NSmartProxy.svg)](https://github.com/tmoonlight/NSmartProxy/blob/master/LICENSE)
[![Build Status](https://dev.azure.com/tmoonlight/NSmartProxy/_apis/build/status/tmoonlight.NSmartProxy?branchName=master)](https://dev.azure.com/tmoonlight/NSmartProxy/_build/latest?definitionId=1&branchName=master)<br />
![Docker Pulls](https://img.shields.io/docker/pulls/tmoonlight/nspclient?label=nspclient%20docker%20pulls)
![Docker Pulls](https://img.shields.io/docker/pulls/tmoonlight/nspserver?label=nspserver%20docker%20pulls)<br />
[中文版](https://github.com/tmoonlight/NSmartProxy/blob/master/README.md) | English

# NSmartProxy

#### What is NSmartProxy?
NSmartProxy is a reverse proxy tool that creates a secure tunnel from a public endpoint to a locally service.

## Characteristics
1. Cross-platform, client and server can run on MacOS, Linux, Windows systems;<br />
2. Easy to use and simple to configure;<br />
3. Multi-end mapping, one NSmartProxy client can map multiple service nodes.

4. Supports all protocols under the TCP protocol stack (such as FTP, Telnet, SMTP, HTTP/HTTPS, POP3, SMB, VNC, RDP. UDP protocol is not supported at present.)

## Operating principle
NSmartProxy contains two service programs:<br />
* Server (NSPServer): Deployed on the external network to receive reverse connections from users and NSPClients and forward them to each other.
* Client (NSPClient): Deployed on the internal network to forward requests and responses to access various services on the intranet.
<img src="https://github.com/tmoonlight/100lines/raw/master/theo_en.png">

## Preparation
#### Linux/Windows/MacOS
1. Install [.NET Core Runtime](https://dotnet.microsoft.com/download)<br />
2. Download the latest version of [NSmartProxy](https://github.com/tmoonlight/NSmartProxy/releases)
#### Docker
* You can run the nspserver directly without having to install the runtime:
```
sudo docker pull tmoonlight/nspclient
sudo docker run --name mynspclient -dit tmoonlight/nspclient
```

## Instructions
NSmartProxy supports various port mappings based on TCP services. The following is an example of nspclient configuration which contains mstsc, iis, and ftp services:<br />
1. Open the appsettings.json file in the installation directory, edit the service address, port,and map-rule as follow:<br />
```
{
  "ProviderWebPort": 12309,			//Configure the port of the NSPServer service
  "ProviderAddress": "2017studio.imwork.net",	//Configure the address of the NSPServer service

  //NSPClients, you can configure multiple
  "Clients": [
    {
      "IP": "127.0.0.1",           //Reverse proxy machine ip
      "TargetServicePort": "3389"  //Port of the reverse proxy service
      "ConsumerPort":"3389"          //External network access port, if occupied,the nspclient will allocate ports in order from 20000
    },
    {
      "IP": "127.0.0.1",
      "TargetServicePort": "80"
    },
    {
      "IP": "127.0.0.1",
      "TargetServicePort": "21"
    }
  ]
}
```
<br />
2. Run NSmartProxy <br />

* Linux：
```
    sudo unzip client.zip
    cd client
    sudo dotnet NSmartProxyClient.dll
```
* Windows：

	Unzip nspclient*.zip and run NSmartProxyWinform.exe:
<img src="https://github.com/tmoonlight/100lines/raw/master/nsprrunnning_2_en.gif" />

* P.S： The above is the configuration method of the client. In general, you can use the free service (2017studio.imwork.net) to perform intranet mapping. If you want to build the NSmartProxy server yourself, please click [here](https://github.com/tmoonlight/NSmartProxy/blob/master/README_SERVER.md).

## Use Cases
We have already described the method of mapping the services of the intranet to the external network, and there are more interesting usages waiting for you to 
discover:<br />
1.Remote boot
<br />
2.Use windows remote control to operate the office computer
<br />
3.Say goodbye to expensive vps and make a more powerful service cluster at a very low cost<br />
...etc
<br />
