#include "config.c"
#include "utils.c"
#include "MqttClient.h"

void setup() {
  Serial.begin(9600);     // Communication with the computer

  // Disable the Mega's microcontroller to avoid interference
  pinMode(10, OUTPUT);
  digitalWrite(10, HIGH);  // Pull the pin high to disable the Mega's microcontroller

  MqttClient* mqtt = new MqttClient(Serial1);
  
  mqtt->connectWifi();
}

void loop() {
  if (Serial.available()) {  // Data from the computer to the ESP-01
    Serial1.write(Serial.read());
  }
  if (Serial1.available()) {  // Data from the ESP-01 to the computer
    Serial.write(Serial1.read());
  }
}