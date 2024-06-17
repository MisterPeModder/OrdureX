#include "config.h"
#include "at.h"
#include "binary.h"
#include "scheduler.h"

Scheduler scheduler;

// Example request, will be deleted
void exampleRequestsRaw() {
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

  addSendData(invalid_code0, 17);
  addSendData(invalid_code2, 17);
  addSendData(invalid_code2, 17);
  // send 3 status
  send(nullptr);
  addSendData(invalid_code0, 17);
  // send 1 status
  send(nullptr);
  // send nothing
  send(nullptr);
}

/**
 * Example requests with 
 */
void exampleRequests(void* context) {
  // invalide code trash 0: array size is 16
  // byte client_id[] = {
  //   0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7, 0x98,
  //   0x91, 0x47
  // };

  addSendData(trash0CollectRequested(), 1);
  //addSendData(trash1CollectRequested(), 1);
  //addSendData(trash2CollectRequested(), 1);

  //addSendData(trash0InvalidCode(client_id), 17);
  //at->addSendData(trash1InvalidCode(client_id), 17);
  //at->addSendData(trash2InvalidCode(client_id), 17);

  //at->addSendData(trash1Burning(), 1);
  //at->addSendData(trash0LidS(true), 2);
  //at->addSendData(trash2LidS(false), 2);

  // client_id[5] = 0xff;
  //at->addSendData(simulationS(true, client_id), 18);

  send(nullptr);
}

void setup() {
  // Communication with the computer
  Serial.begin(115200);
  // Communication with the ESP
  Serial1.begin(115200);

  connectWifi();
  connectRelay();

  task_t task = { .delay = 8000, .lastRun = millis(), .loop = true, .task = exampleRequests, .context = nullptr };
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Send to ESP task not added");
#endif
  }

  task.task = receive;
  task.delay = 1000;
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Receive from ESP task not added");
#endif
  }
}

void loop() {
  if (Serial.available()) {  // Data from the computer to the ESP-01
    Serial1.write(Serial.read());
  }
  // if (Serial1.available()) {  // Data from the ESP-01 to the computer
  //   Serial.write(Serial1.read());
  // }

  scheduler.run();
}