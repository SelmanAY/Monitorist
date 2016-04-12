mkdir ./Monitorist/Deploy
mkdir ./Monitorist/Deploy/Collectors
mkdir ./Monitorist/Deploy/configs
mkdir ./Monitorist/Deploy/Senders

cp -avr ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* ./Monitorist/Deploy/
cp -avr ./Monitorist/Monitorist.Pump.Service/Collectors/*.* ./Monitorist/Deploy/Collectors/
cp -avr ./Monitorist/Monitorist.Pump.Service/configs/*.* ./Monitorist/Deploy/configs/
cp -avr ./Monitorist/Monitorist.Pump.Service/Senders/*.* ./Monitorist/Deploy/Senders/

cp -avr ./Monitorist/Monitorist.Pump.Collectors/bin/Release/*.dll ./Monitorist/Deploy/Collectors
cp -avr ./Monitorist/Monitorist.Pump.GraphiteSender/bin/Release/*.dll ./Monitorist/Deploy/Senders

ls -alR ./Monitorist/Deploy/*.* > ./Monitorist/Deploy/fileList.txt

7z a travisBuild.zip ./Monitorist/Deploy/*.* -r
