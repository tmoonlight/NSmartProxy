
<img src="https://github.com/tmoonlight/NSmartProxy/blob/img/NSmaryProxy.png">

# NSmartProxy

#### 什么是NSmartProxy？<br />
NSmartProxy是一款免费的内网穿透工具。

## 特点
1. 跨平台，客户端和服务端均可运行在MacOS，Linux，Windows系统上；<br />
2. 使用方便，配置简单；<br />
3. 多端映射，一个NSmart Proxy客户端可以同时映射多种服务。（暂不支持UDP协议，开发中。）

## 运行原理
NSmartProxy包含两个服务程序：<br />
* 服务端（NSmartServer）：部署在外网，用来接收来自最终使用者和客户端的反向连接，并将它们进行相互转发。
* 客户端（NSmartClientRouter）：部署在内网，用来转发访问内网各种服务的请求以及响应。
<img src="https://github.com/tmoonlight/NSmartProxy/blob/img/theo.png">

## 启动准备
#### Linux
1. 安装[.NET Core环境](https://dotnet.microsoft.com/download/linux-package-manager/rhel/runtime-current)<br />
2. 下载[NSmartProxy For Linux](https://https://github.com/tmoonlight)

#### windows
1. 下载[.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework/net461)<br />
2. 下载[NSmartProxy For Windows](https://github.com/tmoonlight/NSmartProxy/releases/download/0.1/NSmartProxy_Client_V0_1_net4_6_1.zip)

## 使用方法
NSmartProxy支持各种基于TCP服务的端口映射，下面以mstsc,iis,ftp服务为例：<br />
1. 打开安装目录下的appsetting.json文件，配置服务地址，映射地址和端口：<br />
```
{
  "ProviderPort": "9974",                     //反向连接的端口
  "ProviderConfigPort": "12308",              //配置服务的端口
  "ProviderAddress": "2017studio.imwork.net", //配置服务的地址，可以是域名（eg.:domain.com）也可以是ip（eg.:211.5.5.4）
  //"ProviderAddress": "192.168.0.106",

  //反向代理客户端，可以配置多个
  "Clients": [
    {
      "IP": "127.0.0.1",           //反向代理机器的ip
      "TargetServicePort": "3389"  //反向代理服务的端口
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
    sudo unzip NSmartProxy_Client_V0_1_netcore.zip
    cd NSmartProxy_Client_V0_1_netcore
    sudo dotnet NSmartProxyClient.dll
```
* Windows：

	解压NSmartProxy_Client_V0_1_net4_6_1.zip，运行NSmartProxyClient.exe即可

* P.S： 以上是客户端的配置方法，一般情况下，只要用我的免费服务（2017studio.imwork.net）即可进行内网映射了，如果您还想自己搭建NSmartProxy服务端，请参考[这里](https://github.com/tmoonlight/NSmartProxy/blob/master/README_SERVER.md)。

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
