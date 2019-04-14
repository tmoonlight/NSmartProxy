
<img src="https://github.com/tmoonlight/NSmartProxy/blob/img/NSmaryProxyNew.png">

# NSmartProxy ServerHost

这里介绍NSmartProxy服务端的安装方法（linux,windows,MacOS均适用）<br />

## 启动准备
* 首先你需要一台具备独立IP的服务器，以下安装过程均在此机器上执行：
#### Linux/Windows/MacOS
1.安装[.NET Core环境](https://dotnet.microsoft.com/download)<br />
2.下载最新版的[NSmartProxy](https://github.com/tmoonlight/NSmartProxy/releases)

## 使用方法
1. 解压缩NSmartProxy服务端的压缩包。
2. 打开安装目录下的appsettings.json文件，设置反向连接端口和配置服务端口：<br />
```
{
  "ClientServicePort": 9974,      //反向连接端口
  "ConfigServicePort": 12308      //配置服务端口
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



