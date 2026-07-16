@echo off
net stop TSMQService
sc delete TSMQService
pause
