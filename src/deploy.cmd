rem *** 个人用的部署到树莓派的脚本 ***
xcopy  %~dp0..\build\nspserver_v1.2 z:\nsmart /y /e /i
curl "http://2017studio.imwork.net:7002/index.html?processname=nspserver&action=restart"
pause