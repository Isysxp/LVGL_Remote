# LVGL_Remote. A remote LVGL screen viewer for an ESP32 server

I built this app more for idle curiosity rather then being practically useful. It is effectively
a VNC like client app to connect to a VNC like server app running on an ESP32. The primary use case for
LVGL is to create a responsive UI on a local display device eg a TFT. There are numerous examples as to
hwo this may be achieved. In the case, the intention was to run the ESP32 Lvgl app 'headless' just as
in the common case of a Raspberry Pi. Then the virtual display may be seen in the client app with a mouse
and/or keyboard interface. Initially I added some simple code to an LVGL app to send the virtual screen
back to the client. At this point I discovered EgonBeermat's server and client code: https://github.com/egonbeermat/RemoteDisplay/tree/0.3.0-dev
which exactly fitted the bill. I then updated this app to match the protocol used by EgonBeermat. This uses
a straight forward RLE encoding of the data in the ESP32's display buffer and sends this data as a series of
UDP datagrams. This data is then decoded in an asynchronous callback in response to data being received by
A UDP client. In addition, another UDP client using the same port sends mouse data back to the ESP32 app.
The amount of data sent is quite small and depends on the LVGL display update module which only send new
data from one of more regions of the display which have been marked as changed. Not surprisingly, the LVGL
developers have spent some time minimising the amount of data that needs to be sent such that a typical
TFT module can be updated quite quickly.
This app have been specifically configured to:
1. The app expects the ESP32 server to be registered with DNS with the name 'Calculator'
2. The USB port is set to 2400.
3. The app displays a PictureBox of size 800x480 and this is used in the definition of the ESP32 display device.
I have included in this repo an example Arduino app for an ESP32 device (a Calculator). This device does not need an attached
TFT. I strongly suggest using an ESP32-S3. You will need to upadate this test app. Specifically:
1. Adjust the SSID and Password to match your WiFi network.
2. In the Arduino IDE check for the following libraries:<br>
   a. LovyanGFX 1.2.19<br>
   b. LVGL 8.4. NB this is NOT the latest version. Unfortunately, for 9.X, the devs have moved the goal posts around quite a bit!<br>
Also, I have included the designer files for the Calculator that may be edited with EEZ studio: https://www.envox.eu/studio/studio-introduction/
I think that this is a really cool app.
<br>
Finally, I would like to thank EgonBeermat for using his code and note that the Arduino app is also compatible with his Remote Display app.<br>
<br>
Ian Schofield<br>
April 2026<br>


