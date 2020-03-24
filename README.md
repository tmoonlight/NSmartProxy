

<img src="http://2017studio.oss-accelerate.aliyuncs.com/NSmartProxyNew.png">

[![GitHub
release](https://img.shields.io/github/release/tmoonlight/NSmartProxy.svg?logoColor=%21%5BGitHub%20release%5D%28https%3A%2F%2Fimg.shields.io%2Fgithub%2Frelease%2Ftmoonlight%2FNSmartProxy.svg%29)](https://github.com/tmoonlight/NSmartProxy/releases)
[![GitHub](https://img.shields.io/github/license/tmoonlight/NSmartProxy.svg)](https://github.com/tmoonlight/NSmartProxy/blob/master/LICENSE)
[![Build
Status](https://dev.azure.com/tmoonlight/NSmartProxy/_apis/build/status/tmoonlight.NSmartProxy?branchName=master)](https://dev.azure.com/tmoonlight/NSmartProxy/_build/latest?definitionId=1&branchName=master)
<br />
![Docker Pulls](https://img.shields.io/docker/pulls/tmoonlight/nspclient?label=nspclient%20docker%20pulls)
![Docker Pulls](https://img.shields.io/docker/pulls/tmoonlight/nspserver?label=nspserver%20docker%20pulls)<br />
中文版 \|
[English](https://github.com/tmoonlight/NSmartProxy/blob/master/README_EN.md)

NSmartProxy
===========

#### 什么是NSmartProxy？<br />

NSmartProxy是一款免费的内网穿透工具。<br />
使用中如果有任何问题和建议，可以[点击这里加入Gitter群组](https://gitter.im/tmoonlight/NSmartProxy)或者[点击这里加入QQ群
（群号：813170640）](//shang.qq.com/wpa/qunwpa?idkey=139dc3d01be5cc7ac3226c022d832b8ddcc4ec4b64d8755cd4f5c669994970c7)我们一起讨论。

目录
----
 -   [特点](#特点)
 -   [运行原理](#运行原理)
 -   [客户端安装](#客户端安装)
     -   [启动准备](#启动准备)
     -   [使用方法](#使用方法)
 -   [服务端安装](#服务端安装)
     -   [启动准备](#启动准备-1)
     -   [使用方法](#使用方法-1)
 -   [使用案例](#使用案例)

特点
----

1.  跨平台，客户端和服务端均可运行在MacOS，Linux，Windows系统上；<br />
2.  使用方便，配置简单；<br />
3.  多端映射，只需安装一个NSmartProxy客户端可映射整个局域网内的多种服务；
4.  支持TCP协议栈下的所有协议（已经经过测试的有FTP、Telnet、SMTP、HTTP/HTTPS、POP3、SMB、VNC、RDP。），以及相当一部分基于UDP的协议（已经经过测试的有DNS查询、mosh服务）。

运行原理
--------

NSmartProxy包含两个服务程序：<br /> 
* 服务端（NSmartProxy.ServerHost）：部署在外网，用来接收来自最终使用者和客户端的反向连接，并将它们进行相互转发。
* 客户端（NSmartProxyClient）：部署在内网，用来转发访问内网各种服务的请求以及响应。
<img src="http://2017studio.oss-accelerate.aliyuncs.com/theo.png">

客户端安装
----------

NSmartProxy支持各种基于TCP和UDP服务的端口映射，下面以mstsc,iis,ftp以及mosh服务为例：<br />

### 启动准备

NSmartProxy的客户端被打包成三种发布方式：第一种是跨平台包，需要预先安装[.NET
Core环境](https://dotnet.microsoft.com/download)。
第二种是SCD包（包名带"scd"），无需安装.net环境，用户需要根据自己的平台和架构选择相应的压缩包。第三种是Windows窗体版本（包名带"winform"）：
#### Windows 
1. 确保客户端的环境在.NET Framework 4.6.1 以上。 
2. 下载最新的窗体版本https://github.com/tmoonlight/NSmartProxy/releases/download/v1.2_final/nspclient_winform_v1.2.zip

#### Linux

-  下载最新版本的NSmartProxyClient，以SCD发布下的linux x64系统为例：

<!-- -->

    wget https://github.com/tmoonlight/NSmartProxy/releases/download/v1.2_final/nspclient_scd_linux_v1.2.zip

#### MacOS

-  下载最新版本的NSmartProxyClient：

<!-- -->

    wget https://github.com/tmoonlight/NSmartProxy/releases/download/v1.2_final/nspclient_scd_osx_v1.2.zip

#### Docker

-   如果当前机器上已经有了docker运行环境，则无需安装运行时，直接拉取镜像即可运行，如下脚本在Docker
    CE 17.09下测试通过：

<!-- -->

    sudo docker pull tmoonlight/nspclient
    sudo docker run --name mynspclient -dit tmoonlight/nspclient

### 使用方法

1.  打开安装目录下的appsettings.json文件，配置服务地址，映射地址和端口（winform版本也兼容这种配置方式，也可直接进入界面配置）：<br />

<!-- -->

    {
      "ProviderWebPort": 12309,         //服务器端口
      "ProviderAddress": "2017studio.imwork.net",   //服务器地址

      //反向代理客户端列表
      "Clients": [
        {//mstsc远程控制服务
          "IP": "127.0.0.1",           //反向代理机器的ip
          "TargetServicePort": "3389"  //反向代理服务的端口
          "ConsumerPort":"3389"          //外网访问端口，如被占用，则会从20000开始按顺序分配端口
        },
        {//网站服务
          "IP": "127.0.0.1",
          "TargetServicePort": "80"
        },
        {//ftp服务
          "IP": "127.0.0.1",
          "TargetServicePort": "21",
          "IsCompress" : true,      //表示启动传输压缩
          "Description": "这是一个ftp协议。" //描述字段，方便用户在服务端界面识别
        },
        {//mosh服务 
          "IP": "192.168.0.168",    //安装mosh服务的受控端地址
          "TargetServicePort": "60002",
          "ConsumerPort": "30002",  
          "Protocol": "UDP"     //表示是一个UDP协议，如果不加以配置，则以TCP协议来转发
        }
      ]
    }

<br /> 2. 运行NSmartProxy客户端 <br />

-   Linux：

<!-- -->

        sudo unzip nspclient_scd_linux_v1.2.zip
        cd nspclient_scd_linux_v1.2
        chmod +x ./NSmartProxyClient
        ./NSmartProxyClient

-   MacOS：

<!-- -->

        sudo unzip nspclient_osx_linux_v1.2.zip
        cd nspclient_scd_osx_v1.2
        chmod +x ./NSmartProxyClient
        ./NSmartProxyClient

-   Windows： 解压后运行NSmartProxyWinform.exe即可:

    <img src="http://2017studio.oss-accelerate.aliyuncs.com/5.nspclientwinformrunning.gif" />
    <br />

3.  后台运行：<br />
    您还可以将NSmartProxy客户端注册为一个后台服务，方法如下：

-   Windows：<br /> 
    - 方法一<br />
    <img src="https://github.com/tmoonlight/NSmartProxy/raw/master/imgs/servicecn.png"><br />

    - 方法二<br />
```
    rem 注册客户端windows服务
    .\NSmartProxyClient action:install
```
```
    rem 卸载客户端windows服务
    .\NSmartProxyClient action:uninstall
```
-   MacOS/Linux 暂略

-   P.S：
    以上是客户端的配置方法，一般情况下，只要用我的免费服务（2017studio.imwork.net）即可进行内网映射了，如果你还想自己搭建服务端，请接着往下看。

服务端安装
----------

这里介绍NSmartProxy服务端的安装方法（linux,windows,MacOS均适用）<br />

### 启动准备

-   首先你需要一台具备独立IP的服务器，以下安装过程均在此机器上执行：
#### Linux/Windows/MacOS

1.  NSmartProxy的服务端程序被打包成两种发布方式。第一种是跨平台包，需要预先安装[.NET
    Core环境](https://dotnet.microsoft.com/download)。
    第二种是SCD包（包名带"scd"），无需安装.net环境，用户需要根据自己的平台和架构选择相应的压缩包。<br />
2.  下载最新版的NSmartProxy服务端：
-   Linux：
<!-- -->
    wget https://github.com/tmoonlight/NSmartProxy/releases/download/v1.2_final4/nspserver_scd_linux_v1.2_final4.zip

-   Windows：<br />
下载https://github.com/tmoonlight/NSmartProxy/releases/download/v1.2_final4/nspserver_scd_win_v1.2_final4.zip

-   MacOS：
<!-- -->

    wget https://github.com/tmoonlight/NSmartProxy/releases/download/v1.2_final4/nspserver_scd_osx_v1.2_final4.zip

#### Docker

-   无需安装运行时，直接拉取镜像即可运行，运行镜像时需要4组端口：配置端口，反向连接端口，API服务端口，以及使用端口，如下脚本在Docker
    CE 17.09下测试通过：

<!-- -->

    sudo docker pull tmoonlight/nspserver
    sudo docker run --name mynspserver -dit -p 7842:7842 -p 7841:7841 -p 12309:12309 -p 20000-20050 tmoonlight/nspserver

### 使用方法

1.  解压缩NSmartProxy服务端的压缩包，以下以SCD发布下的linux系统为例

<!-- -->

    unzip nspserver_scd_linux_v1.2_final4.zip

2.  打开安装目录下的appsettings.json文件，设置反向连接端口和配置服务端口，如果没有特殊需求，默认就好：<br />

<!-- -->

    {
      "ReversePort": 7842, //反向连接端口
      "ConfigPort": 7841, //配置服务端口
      "WebAPIPort": 12309         //API服务端口
    }

3. 运行NSmartProxy <br />

第一步 cd到安装目录 <br /> 第二步 执行以下命令 
* Linux/MacOS：

    chmod +x ./NSmartProxy.ServerHost
    ./NSmartProxy.ServerHost

* Windows：
点击 Win+R 打开运行窗口. 输入 "cmd" 按下Ctrl+Shift+Enter打开管理员身份运行的命令行窗口。cd到安装目录，运行如下指令：

    NSmartProxy.ServerHost

第三步 登陆http://ip:12309 进入web端，出厂用户密码为admin/admin

<img src="http://2017studio.oss-accelerate.aliyuncs.com/6.nspserverrunnning_1.gif" />

第四步 进入服务端对用户进行各种管理操作

<img src="http://2017studio.oss-accelerate.aliyuncs.com/6.nspserverrunnning_2.gif" />

####   注册为后台服务<br />
    NSmartProxy客户端和服务端均可以注册为一个后台服务，方法如下：
* Windows
    以管理员身份打开命令行后，cd到程序运行目录，运行以下指令进行服务的注册和卸载：

<!-- -->

    rem 注册服务端windows服务
    .\NSmartProxy.ServerHost action:install

    rem 卸载服务端windows服务
    .\NSmartProxy.ServerHost action:uninstall

* MacOS/Linux <br />
可参考wiki: [How To: 30秒使用Linux搭建一个内网穿透服务端](https://github.com/tmoonlight/NSmartProxy/wiki/How-To:-30%E7%A7%92%E4%BD%BF%E7%94%A8Linux%E6%90%AD%E5%BB%BA%E4%B8%80%E4%B8%AA%E5%86%85%E7%BD%91%E7%A9%BF%E9%80%8F%E6%9C%8D%E5%8A%A1%E7%AB%AF)

使用案例
--------
以上已经讲述了将内网的服务映射到外网的方法，还有更多有趣的用法等着你发掘：<br />
1. 远程开机 
2. [使用windows远程控制操作办公室电脑](https://github.com/tmoonlight/NSmartProxy/wiki/How-To:-%E4%BD%BF%E7%94%A8NSmartProxy%E5%AE%9E%E7%8E%B0windows%E4%B8%8A%E7%9A%84%E8%BF%9C%E7%A8%8B%E5%8A%9E%E5%85%AC) 
3. 告别昂贵的vps，以极低的成本制作一个更强大的服务集群<br />
