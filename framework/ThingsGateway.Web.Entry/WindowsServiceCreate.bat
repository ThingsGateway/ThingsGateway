cd ..
sc create ThingsGateway binPath=%~dp0ThingsGateway.Web.Entry.exe start= auto 
sc description ThingsGateway "ThingsGatewayÎïÁªÍø¹Ø"
Net Start ThingsGateway
pause
