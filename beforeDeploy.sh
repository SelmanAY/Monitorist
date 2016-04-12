mkdir ./Monitorist/Deploy
cp -avr ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* ./Monitorist/Deploy/
cp -avr ./Monitorist/Monitorist.Pump.Collectors/bin/Release/*.dll ./Monitorist/Deploy/Collectors
cp -avr ./Monitorist/Monitorist.Pump.GraphiteSender/bin/Release/*.dll ./Monitorist/Deploy/Senders

ls -alR ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* > ./Monitorist/Deploy/releaseFileList.txt
ls -alR ./Monitorist/Deploy/*.* > ./Monitorist/Deploy/fileList.txt

zip -r ./deploy.zip ./Monitorist/Deploy/*.*