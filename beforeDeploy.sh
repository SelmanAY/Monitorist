mkdir ./Monitorist/Pump
mkdir ./Monitorist/Pump/Collectors
mkdir ./Monitorist/Pump/configs
mkdir ./Monitorist/Pump/Senders

cp -avr ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* ./Monitorist/Pump/
cp -avr ./Monitorist/Monitorist.Pump.Service/Collectors/*.* ./Monitorist/Pump/Collectors/
cp -avr ./Monitorist/Monitorist.Pump.Service/configs/*.* ./Monitorist/Pump/configs/
cp -avr ./Monitorist/Monitorist.Pump.Service/Senders/*.* ./Monitorist/Pump/Senders/

cp -avr ./Monitorist/Monitorist.Pump.Collectors/bin/Release/*.dll ./Monitorist/Pump/Collectors
cp -avr ./Monitorist/Monitorist.Pump.GraphiteSender/bin/Release/*.dll ./Monitorist/Pump/Senders

find ./Monitorist/Pump/ -type f -name '*.mdb' -delete
find ./Monitorist/Pump/ -type f -name '*.xml' -delete

7z a travisBuild.zip ./Monitorist/Pump/ -r
