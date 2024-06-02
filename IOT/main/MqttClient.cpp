#include "MqttClient.h"
#include "config.c"
#include "utils.c"

MqttClient::MqttClient(HardwareSerial& serial) {
  this->serial = &serial;
  this->serial->begin(115200);
  this->serial->println("AT+RESTORE");
  delay(1000);
  this->serial->println("AT+CWMODE=1");
  delay(1000);
}

void MqttClient::connectWifi() {
  const char* wifiConnexion = concat5("AT+CWJAP=\"", WIFI_SSID, "\",\"", WIFI_PASSWORD, "\"");
  Serial1.println(wifiConnexion);
  free((void*)wifiConnexion);
  delay(5000);
}
