
<img src="https://github.com/tmoonlight/NSmartProxy/blob/img/NSmaryProxy.png">

# NSmartProxy ServerHost

这里介绍NSmartProxy服务端的安装方法（linux,windows均适用）<br />

## 启动准备
#### Linux
1.安装[.NET Core环境](https://dotnet.microsoft.com/download)<br />
2.下载[NSmartProxyServer](https://github.com/tmoonlight/NSmartProxy/releases/download/0.1/NSmartProxy_ServerHost_V0_1_netcore2_1.zip)

## 使用方法
1. 解压缩NSmartProxy服务端的压缩包。
2. 打开安装目录下的appsetting.json文件，设置反向连接端口和配置服务端口：<br />
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
* Linux：
```
chmod ./run.sh 555
./run.sh
```
* Windows：

```
运行安装目录下的run.bat
```



