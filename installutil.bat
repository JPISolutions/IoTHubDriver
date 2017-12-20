@echo off
cls
copy /Y C:\source\IoTHubDriver\DBModule\bin\Debug\JPIDBObjIotHub.dll "C:\Program Files\Schneider Electric\ClearSCADA\JPIDBObjIotHub.dll" 
cd "C:\Program Files\Schneider Electric\ClearSCADA"
%WinDir%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil JPIDBObjIotHub.dll
cd C:\source\IoTHubDriver