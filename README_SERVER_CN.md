
<img src="https://github.com/tmoonlight/NSmartProxy/blob/master/NSmartProxyNew.png">

# NSmartProxy ServerHost

这里介绍NSmartProxy服务端的安装方法（linux,windows,MacOS均适用）<br />

## 启动准备
* 首先你需要一台具备独立IP的服务器，以下安装过程均在此机器上执行：
#### Linux/Windows/MacOS
1.安装[.NET Core环境](https://dotnet.microsoft.com/download)<br />
2.下载最新版的[NSmartProxy](https://github.com/tmoonlight/NSmartProxy/releases

#### Docker
* 无需安装运行时，直接拉取镜像，即可运行，运行镜像时需要4组ip，分别对应服务端配置中的配置端口，反向连接端口，API服务端口，以及使用端口 ：
```
sudo docker pull tmoonlight/nspserver
sudo docker run --name mynspserver -dit -p 12308:12308 -p 12309:12309 -p 19974:19974 -p 20000-20050 tmoonlight/nspserver
```

## 使用方法
1. 解压缩NSmartProxy服务端的压缩包。
2. 打开安装目录下的appsettings.json文件，设置反向连接端口和配置服务端口：<br />
```
{
  "ReversePort": 7842, //反向连接端口
  "ConfigPort": 7841, //配置服务端口
  "WebAPIPort": 12309         //API服务端口
}
```
<br />
3. 运行NSmartProxy <br />

第一步 cd到安装目录 <br />
第二步 执行以下命令
* Linux/MacOS：
```
sudo dotnet NSmartProxy.ServerHost.dll
```
* Windows：

```
运行安装目录下的run.cmd
```

第三步 登陆http://ip:12309 进入web端，出厂用户密码为admin/admin

<img src="https://github.com/tmoonlight/100lines/raw/master/6.nspserverrunnning_1.gif" />

第四步 进入服务端对用户进行各种管理操作

<img src="https://github.com/tmoonlight/100lines/raw/master/6.nspserverrunnning_2.gif" />
