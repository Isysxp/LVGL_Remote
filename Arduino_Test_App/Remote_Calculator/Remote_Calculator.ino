
/*******************************************
	Board https://www.waveshare.com/wiki/ESP32-S3-Touch-LCD-4.3
	Flash size 8Mb
	OPI PSRAM

	board manager : esp32 by ESpressif 3.1.1
	Libraries :
	- lvgl by kisvegador 8.4.0
	- LovyanGFX by lovyan03 1.2.7
*******************************************/

#include <lvgl.h>
#include "displayStuff.h"
#include <string.h>
#include "src/ui/vars.h"
#include "src/ui/actions.h"
#include "src/ui/screens.h"
#include "src/ui/ui.h"
#include <stdio.h>
#include <math.h>
#include <Esp.h>
#include <WiFi.h>
#include "EspUsbHost.h"
#include "MouseLVGL.h"
#include "RemoteDisplay.h"
extern RemoteDisplay remoteDisplay;

#define INFRA_SSID "BT-Q6CTR8"
#define INFRA_PSWD "c531a3d358"

static float rslt = 0.0;
static int inptr = 0;
static char strslt[32];
extern float addsub();
extern char input[101];
int packetSize;
int packetBuffer[2];

void action_button_clicked(lv_event_t* e) {
	const char* label_text;
	float rslt;


	lv_event_code_t code = lv_event_get_code(e);  // Get the event code eg LV_EVENT_PRESSED
	lv_obj_t* obj = lv_event_get_target(e);       // Get a reference to the widget generating this event
	if (code == LV_EVENT_PRESSED) {
		uint32_t id = lv_btnmatrix_get_selected_btn(obj);  // Get the widget (keypad) id number
		label_text = lv_btnmatrix_get_btn_text(obj, id);   // Get the text label inside the specific widget.
		if (strstr(input, "="))
			input[0] = 0;
		strcat(input, label_text);
		switch (*label_text) {
			case 'C':
				lv_textarea_set_text(objects.textarea1, "");  // Directly reference a specific widget (textarea1) see screens.h
				*input = *strslt = 0;                         // Clear input and result text
				return;
			case '+':
			case '-':
			case '/':
			case '*':
				if (*strslt) {  // If there is a result, set it as first operand
					strcpy(input, strslt);
					strcat(input, label_text);
				}
				break;
			case '=':
				if (*input == '=')
					return;
				rslt = addsub();
				sprintf(strslt, "%.7g", rslt);
				strcat(input, strslt);
				break;
			default:
				*strslt = 0;
		}
		lv_textarea_set_text(objects.textarea1, (const char*)&input);
	}
}

lv_disp_t* disp;
int count = 0;

void setup() {
	Serial.begin(115200);

	//tft.begin();  // Graphics init code for the ST7262+CHG422 touch screen. (see displaystuff.h as well)

	lv_init();
	lv_disp_draw_buf_init(&draw_buf, buf, NULL, screenWidth * 10);
	static lv_disp_drv_t disp_drv;
	lv_disp_drv_init(&disp_drv);
	disp_drv.hor_res = screenWidth;
	disp_drv.ver_res = screenHeight;
	disp_drv.flush_cb = my_disp_flush;
	disp_drv.draw_buf = &draw_buf;
	disp = lv_disp_drv_register(&disp_drv);
	/*
	static lv_indev_drv_t touch_drv;
	lv_indev_drv_init(&touch_drv);
	touch_drv.type = LV_INDEV_TYPE_POINTER;
	touch_drv.read_cb = my_touch_read;
	lv_indev_drv_register(&touch_drv);
	*/
	// Set display rotation to any of
	// LV_DISP_ROT_NONE, LV_DISP_ROT_90, LV_DISP_ROT_180, LV_DISP_ROT_270
	//disp_drv.sw_rotate = 1;
	//disp_drv.rotated = LV_DISP_ROT_NONE;
	input[0] = strslt[0] = 0;  // Clear input and result text
	ui_init();
	setup_cursor();

	WiFi.disconnect(true);
	delay(1000);
	WiFi.useStaticBuffers(true);
	WiFi.setMinSecurity(WIFI_AUTH_WPA_PSK);
	WiFi.setHostname("calculator");
	WiFi.mode(WIFI_STA);
	WiFi.begin(INFRA_SSID, INFRA_PSWD);
	delay(1000);
	WiFi.setTxPower(WIFI_POWER_8_5dBm);
	while (WiFi.status() != WL_CONNECTED) {
		delay(100);
	}
	IPAddress ip = WiFi.localIP();
	Serial.print("IP:");
	Serial.print(ip);
	remoteDisplay.registerTouchCallback(remoteTouchCallback);
	remoteDisplay.registerRefreshCallback(refreshDisplayCallback);
	remoteDisplay.init(800, 480, 2400);
	Serial.println("/Calculator");
	//udp.onPacket(onPacket);
	//udp.listen(1000);
	Mouse.begin();
	Mouse.useLVGL();
	Serial.println("Running....");
}

void loop() {
	lv_timer_handler();
	ui_tick();
	delay(5);
	Mouse.loop();
	remoteDisplay.pollRemoteCommand();
}
