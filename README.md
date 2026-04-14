# LVGL_Remote. A remote LVGL screen viewer for an ESP32 server

I built this app more for idle curiosty rather then being practicaly useful. It is effectively
a VNC like client app to connect to a VNC like server app running on an ESP32. The primary use case for
LVGL is to create a responsive UI on a local display device eg a TFT. There are numerous examples as to
hwo this may be achieved. In the case, the intention was to run the ESP32 Lvgl app 'headless' just as
in the common case of a Raspberry Pi. Then the virtual display may be see in the client app with a mouse
and/ot keyboard interface. Initially I added some simple code to an LVGL app to send the virtual screen
back to the client. At this point I discovered EgonBeermat's server and client code: https://github.com/egonbeermat/RemoteDisplay/tree/0.3.0-dev
which exactly fitted the bill. I then updated this app to match the protocol used by EgonBeermat. This uses
a straight forward RLE encoding of the data in the ESP32's display buffer and sends this data as a series of
UDP datagrams. This data is then decoded in an asynchronous callback in response to data being received by
A UDP client. In addition, another UDP client using the same port sends mouse data back to the ESP32 app.
The amount of data sent is quite small and depends on the LVGL display update module which only send new
data from one of more regions of the display which have been marked as changed. Not surprsingly, the LVGL
developers have spent some time minimising the amount of data that needs to be sent such that a typical
TFT module can be updated quite quickly.
This app have been specifically configured to:
1. The app expects the ESP32 server to be registered with DNS with the name 'Calulator'
2. The USD port is set to 2400.
3. The app displays a PictureBox of size 800x480 and this is used to define the ESP32 display device.
