@echo off

REM Lancement de l'executable du Proxy/Cache
start "Proxy/cache" "%~dp0\ProxyService\bin\Debug\ProxyService.exe"


REM Lancement de l'executable du Serveur
start "Serveur Routing" "%~dp0\RoutingServerService\bin\Debug\RoutingServerService.exe\"

REM Lancement du client java
timeout /t 2 /nobreak >nul
java -jar "%~dp0\ProjetJCDecaux\ClientJava\target\Projet_Biking-1-jar-with-dependencies.jar"
 

:end
