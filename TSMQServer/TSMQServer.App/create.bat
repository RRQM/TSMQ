@echo off
cd /d %~dp0
sc create TSMQService binPath= "%~dp0TSMQServer.App.exe" start= auto
sc description TSMQService "TSMQ 消息队列服务"
Net Start TSMQService
pause
