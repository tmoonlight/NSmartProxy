
<img src="https://github.com/tmoonlight/NSmartProxy/raw/master/NSmartProxyNew.png">

[![GitHub release](https://img.shields.io/github/release/tmoonlight/NSmartProxy.svg?logoColor=%21%5BGitHub%20release%5D%28https%3A%2F%2Fimg.shields.io%2Fgithub%2Frelease%2Ftmoonlight%2FNSmartProxy.svg%29)](https://github.com/tmoonlight/NSmartProxy/releases)
[![GitHub](https://img.shields.io/github/license/tmoonlight/NSmartProxy.svg)](https://github.com/tmoonlight/NSmartProxy/blob/master/LICENSE)
[![Build Status](https://dev.azure.com/tmoonlight/NSmartProxy/_apis/build/status/tmoonlight.NSmartProxy?branchName=master)](https://dev.azure.com/tmoonlight/NSmartProxy/_build/latest?definitionId=1&branchName=master)

中文版 | [English](https://github.com/tmoonlight/NSmartProxy/blob/master/README.md)

# NSmartProxy

#### 什么是NSmartProxy？<br />
NSmartProxy是一款免费的内网穿透工具。<br />
使用中如果有任何问题和建议，可以[点击这里加入Gitter群组](https://gitter.im/tmoonlight/NSmartProxy)和我们一起讨论。

## 特点
1. 跨平台，客户端和服务端均可运行在MacOS，Linux，Windows系统上；<br />
2. 使用方便，配置简单；<br />
3. 多端映射，一个NSmart Proxy客户端可以同时映射多种服务。
4. 支持TCP协议栈下的所有协议（已经经过测试的有FTP、Telnet、SMTP、HTTP/HTTPS、POP3、SMB、VNC、RDP。暂不支持UDP协议，开发中。）

## 运行原理
NSmartProxy包含两个服务程序：<br />
* 服务端（NSmartServer）：部署在外网，用来接收来自最终使用者和客户端的反向连接，并将它们进行相互转发。
* 客户端（NSmartClientRouter）：部署在内网，用来转发访问内网各种服务的请求以及响应。
<img src="https://github.com/tmoonlight/NSmartProxy/raw/img/theo.png">

## 启动准备
#### Linux/Windows/MacOS
1. 安装[.NET Core Runtime](https://dotnet.microsoft.com/download)<br />
2. 下载最新版本的[NSmartProxy](https://github.com/tmoonlight/NSmartProxy/releases)
#### Docker
* 如果当前机器上已经有了docker运行环境，则无需安装运行时，直接拉取镜像即可运行 ：
```
sudo docker pull tmoonlight/nspclient
sudo docker run --name mynspclient -dit tmoonlight/nspclient
```

## 使用方法
NSmartProxy支持各种基于TCP服务的端口映射，下面以mstsc,iis,ftp服务为例：<br />
1. 打开安装目录下的appsettings.json文件，配置服务地址，映射地址和端口（windows版本也可直接进入界面配置）：<br />
```
{
  "ProviderWebPort": 12309,			//服务器端口
  "ProviderAddress": "2017studio.imwork.net",	//服务器地址

  //反向代理客户端，可以配置多个
  "Clients": [
    {
      "IP": "127.0.0.1",           //反向代理机器的ip
      "TargetServicePort": "3389"  //反向代理服务的端口
      "ConsumerPort":"3389"          //外网访问端口，如被占用，则会从20000开始按顺序分配端口
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
2. 运行NSmartProxy <br />

* Linux：
```
    sudo unzip client.zip
    cd client
    sudo dotnet NSmartProxyClient.dll
```
* Windows：

	解压nspclient*.zip，运行NSmartProxyWinform.exe即可:
<img src="https://github.com/tmoonlight/100lines/raw/master/5.nspclientwinformrunning.gif" />
3. 后台运行：<br />
您还可以将NSmartProxy客户端注册为一个后台服务，方法如下：

* Linux：

* Windows：
<img src="https://github.com/tmoonlight/NSmartProxy/raw/master/imgs/servicecn.png">


* P.S： 以上是客户端的配置方法，一般情况下，只要用我的免费服务（2017studio.imwork.net）即可进行内网映射了，如果您还想自己搭建NSmartProxy服务端，请参考[这里](https://github.com/tmoonlight/NSmartProxy/blob/master/README_SERVER_CN.md)。

## 使用案例
以上已经讲述了将内网的服务映射到外网的方法，还有更多有趣的用法等着你发掘：<br />
1.远程开机
<br />
2.使用windows远程控制操作办公室电脑
<br />
3.告别昂贵的vps，以极低的成本制作一个更强大的服务集群<br />
4.使用ssh等工具在当事人毫不知情的情况下监控他们的电脑，防止妻子外遇，孩子早恋（比较不推荐）<br />
...etc
<br />
