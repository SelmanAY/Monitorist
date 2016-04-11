mkdir ./Monitorist/Deploy
cp -avr ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* ./Monitorist/Deploy/
cp -avr ./Monitorist/Monitorist.Pump.Collectors/bin/Release/*.dll ./Monitorist/Deploy/Collectors
cp -avr ./Monitorist/Monitorist.Pump.GraphiteSender/bin/Release/*.dll ./Monitorist/Deploy/Senders

zip -r ./deploy.zip ./Monitorist/Deploy/