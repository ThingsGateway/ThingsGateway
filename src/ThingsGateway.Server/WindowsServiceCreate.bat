cd ..
sc create ThingsGateway binPath= %~dp0ThingsGateway.Server.exe start= auto 
sc description ThingsGateway "ThingsGateway"
Net Start ThingsGateway
pause
