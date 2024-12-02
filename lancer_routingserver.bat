@echo off

:: Vérification des privilèges administrateur
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Ce script nécessite des privilèges administrateur. Veuillez l'exécuter en tant qu'administrateur.
    pause
    exit
)

:: Définir un fichier temporaire pour capturer la sortie d'ActiveMQ
set LOG_FILE="%temp%\activemq_output.log"
if exist %LOG_FILE% del %LOG_FILE%

:: Lancer ActiveMQ avec redirection de sortie
cd /d "C:\Users\pc\Desktop\Web-Front-Development"
start cmd /k "activemq start > %LOG_FILE% 2>&1"

:: Vérifier si ActiveMQ est démarré en surveillant la ligne clé
echo Attente du démarrage complet d'ActiveMQ...

:CHECK_ACTIVEMQ
findstr /c:"INFO | ActiveMQ Jolokia REST API available at http://127.0.0.1:8161/api/jolokia/" %LOG_FILE% >nul
IF %ERRORLEVEL% EQU 0 (
    echo ActiveMQ est démarré.
    goto RUN_ROUTING_SERVER
) ELSE (
    echo ActiveMQ n'est pas encore prêt. Nouvelle tentative dans 5 secondes...
    timeout /t 5 >nul
    goto CHECK_ACTIVEMQ
)

:RUN_ROUTING_SERVER
:: Lancer RoutingServer.exe
start "Routing Server" "C:\Users\pc\Desktop\Web-Front-Development\RoutingServer\RoutingServer\bin\Debug\RoutingServer.exe"

:: Nettoyer le fichier temporaire
del %LOG_FILE%

echo Tout est lancé.
pause
