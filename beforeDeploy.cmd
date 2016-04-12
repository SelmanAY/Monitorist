md ./Monitorist/Deploy

xcopy Monitorist\Monitorist.Pump.Service\bin\Release\*.* Monitorist\Deploy\ /s /e /c /y
xcopy Monitorist\Monitorist.Pump.Service\Collectors\*.* Monitorist\Deploy\Collectors\ /s /e /c /y
xcopy Monitorist\Monitorist.Pump.Service\configs\*.* Monitorist\Deploy\configs\ /s /e /c /y
xcopy Monitorist\Monitorist.Pump.Service\Senders\*.* Monitorist\Deploy\Senders\ /s /e /c /y

xcopy Monitorist\Monitorist.Pump.Collectors\bin\Release\*.dll Monitorist\Deploy\Collectors
xcopy Monitorist\Monitorist.Pump.GraphiteSender\bin\Release\*.dll Monitorist\Deploy\Senders

7z a C:\projects\monitorist\appVeyorBuild.zip Monitorist\Deploy\*.* -r
