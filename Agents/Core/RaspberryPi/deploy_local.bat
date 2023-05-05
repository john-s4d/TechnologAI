@echo on

echo Deploy to 192.168.5.184
scp stream_controller.py pi@192.168.5.184:/home/pi/Technologai/stream_controller.py

echo Deploy to 192.168.5.185
scp stream_controller.py pi@192.168.5.185:/home/pi/Technologai/stream_controller.py

echo Deploy to 192.168.4.85
scp stream_controller.py pi@192.168.4.85:/home/pi/Technologai/stream_controller.py