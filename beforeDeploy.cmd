md ./Monitorist/Pump

xcopy Monitorist\Monitorist.Pump.Service\bin\Release\*.* Monitorist\Pump\ /s /e /c /y
xcopy Monitorist\Monitorist.Pump.Service\Collectors\*.* Monitorist\Pump\Collectors\ /s /e /c /y
xcopy Monitorist\Monitorist.Pump.Service\configs\*.* Monitorist\Pump\configs\ /s /e /c /y
xcopy Monitorist\Monitorist.Pump.Service\Senders\*.* Monitorist\Pump\Senders\ /s /e /c /y

xcopy Monitorist\Monitorist.Pump.Collectors\bin\Release\*.dll Monitorist\Pump\Collectors
xcopy Monitorist\Monitorist.Pump.GraphiteSender\bin\Release\*.dll Monitorist\Pump\Senders

del Monitorist\Pump\*.pdb /a /s
del Monitorist\Pump\*.xml /a /s

7z a appVeyorBuild.zip Monitorist\Pump\*.* -r
