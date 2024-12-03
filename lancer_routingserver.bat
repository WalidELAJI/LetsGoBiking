@echo off

:: Déterminer le chemin du dossier où se trouve le script
set SCRIPT_DIR=%~dp0
cd /d "%SCRIPT_DIR%"

:: Vérification des privilèges administrateur
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Ce script necessite des privileges administrateur. Veuillez l'exécuter en tant qu'administrateur.
    pause
    exit
)

:: Lancer ActiveMQ
start cmd /k "activemq start"

:: Attendre 4 secondes
echo Attente de 4 secondes pour permettre le démarrage complet d'ActiveMQ...
timeout /t 4 >nul

:: Lancer RoutingServer.exe
start "Routing Server" "%SCRIPT_DIR%RoutingServer\RoutingServer\bin\Debug\RoutingServer.exe"

echo Tout est lancé.
pause
