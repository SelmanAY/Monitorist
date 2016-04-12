md ./Monitorist/Deploy
md ./Monitorist/Deploy/Collectors
md ./Monitorist/Deploy/configs
md ./Monitorist/Deploy/Senders

xcopy Monitorist\Monitorist.Pump.Service\bin\Release\*.* Monitorist\Deploy\ /s /e
xcopy Monitorist\Monitorist.Pump.Service\Collectors\*.* Monitorist\Deploy\Collectors\ /s /e
xcopy Monitorist\Monitorist.Pump.Service\configs\*.* Monitorist\Deploy\configs\ /s /e
xcopy Monitorist\Monitorist.Pump.Service\Senders\*.* Monitorist\Deploy\Senders\ /s /e

xcopy Monitorist\Monitorist.Pump.Collectors\bin\Release\*.dll Monitorist\Deploy\Collectors
xcopy Monitorist\Monitorist.Pump.GraphiteSender\bin\Release\*.dll Monitorist\Deploy\Senders

7z a appVeyorBuild.zip Monitorist\Deploy\*.*
