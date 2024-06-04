#include "config.h"
#include "at.h"

void setup() {
  // Communication with the computer
  Serial.begin(9600);
  // Communication with the ESP
  Serial1.begin(115200);

  At* at = new At(Serial1);

  at->connectWifi();
  at->connectRelay();

  // invalide code trash 0: array size is 16
  byte invalid_code0[] = {
    0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7, 0x98,
    0x91, 0x47
  };

  // invalide code trash 2: array size is 16
  byte invalid_code2[] = {
    0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7,
    0x98, 0x91, 0x47
  };

  at->addSendData(trash_0_invalid_code, invalid_code0, 16);
  at->addSendData(trash_2_invalid_code, invalid_code2, 16);
  at->addSendData(trash_1_invalid_code, invalid_code2, 16);

  at->send();
  at->addSendData(trash_1_invalid_code, invalid_code2, 16);
  at->send();
}

void loop() {
  if (Serial.available()) {  // Data from the computer to the ESP-01
    Serial1.write(Serial.read());
  }
  if (Serial1.available()) {  // Data from the ESP-01 to the computer
    Serial.write(Serial1.read());
  }
}