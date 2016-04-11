mkdir ./Deploy
cp ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* ./Deploy/
cp ./Monitorist/Monitorist.Pump.Collectors/bin/Release/*.dll ./Deploy/Collectors
cp ./Monitorist/Monitorist.Pump.GraphiteSender/bin/Release/*.dll ./Deploy/Senders

zip -r ./deploy.zip ./Deploy/