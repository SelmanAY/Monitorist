mkdir ./Deploy
cp ./Monitorist/Monitorist.Pump.Service/bin/Release/*.* ./build/SelmanAY/Monitorist/Deploy/
cp ./Monitorist/Monitorist.Pump.Collectors/bin/Release/*.dll ./build/SelmanAY/Monitorist/Deploy/Collectors
cp ./Monitorist/Monitorist.Pump.GraphiteSender/bin/Release/*.dll ./build/SelmanAY/Monitorist/Deploy/Senders

zip -r ./deploy.zip ./Deploy/