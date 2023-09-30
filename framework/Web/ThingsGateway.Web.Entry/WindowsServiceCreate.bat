cd ..
sc create ThingsGateway binPath=%~dp0ThingsGateway.Web.Entry.exe start= auto 
sc description ThingsGateway "ThingsGateway"
Net Start ThingsGateway
pause
