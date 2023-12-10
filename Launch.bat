@echo off

REM Lancer l'executable du Proxy/Cache
start "Proxy/cache" "%~dp0\ProxyService\bin\Debug\ProxyService.exe"


REM Lancer l'executable du Serveur
start "Serveur Routing" "%~dp0\RoutingServerService\bin\Debug\RoutingServerService.exe\"



:end
