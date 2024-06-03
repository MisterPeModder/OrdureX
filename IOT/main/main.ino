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

  at->dummySend();
}

void loop() {
  if (Serial.available()) {  // Data from the computer to the ESP-01
    Serial1.write(Serial.read());
  }
  if (Serial1.available()) {  // Data from the ESP-01 to the computer
    Serial.write(Serial1.read());
  }
}