#include "config.h"
#include "at.h"
#include "binary.h"

// Example request, will be deleted
void exampleRequestsRaw(At* at) {
  // invalide code trash 0: array size is 17
  byte invalid_code0[] = {
    0x03, 0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7, 0x98,
    0x91, 0x47
  };

  // invalide code trash 2: array size is 17
  byte invalid_code2[] = {
    0x05, 0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7,
    0x98, 0x91, 0x47
  };

  at->addSendData(invalid_code0, 17);
  at->addSendData(invalid_code2, 17);
  at->addSendData(invalid_code2, 17);
  // send 3 status
  at->send();
  at->addSendData(invalid_code0, 17);
  // send 1 status
  at->send();
  // send nothing
  at->send();
}

/**
 * Example requests with 
 */
void exampleRequests(At* at) {
  // invalide code trash 0: array size is 16
  byte client_id[] = {
    0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7, 0x98,
    0x91, 0x47
  };

  at->addSendData(trash0CollectRequested(), 1);
  at->addSendData(trash1CollectRequested(), 1);
  at->addSendData(trash2CollectRequested(), 1);

  at->addSendData(trash0InvalidCode(client_id), 17);
  at->addSendData(trash1InvalidCode(client_id), 17);
  at->addSendData(trash2InvalidCode(client_id), 17);

  at->addSendData(trash1Burning(), 1);
  at->addSendData(trash0LidS(true), 2);
  at->addSendData(trash2LidS(false), 2);

  client_id[5] = 0xff;
  at->addSendData(simulationS(true, client_id), 18);

  delete client_id;

  at->send();
}

void setup() {
  // Communication with the computer
  Serial.begin(9600);
  // Communication with the ESP
  Serial1.begin(115200);

  At* at = new At(Serial1);

  at->connectWifi();
  at->connectRelay();

  exampleRequests(at);
  //exampleRequestsRaw(at);
}

void loop() {
  if (Serial.available()) {  // Data from the computer to the ESP-01
    Serial1.write(Serial.read());
  }
  if (Serial1.available()) {  // Data from the ESP-01 to the computer
    Serial.write(Serial1.read());
  }
}