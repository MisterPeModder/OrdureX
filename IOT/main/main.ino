#include "config.h"
#include "at.h"
#include "bin.h"
#include "binary.h"
#include "scheduler.h"

Scheduler scheduler;

/**
 * Example requests, will be deleted later 
 */
void exampleRequests(void* context) {
  addSendData(trash0CollectRequested(), 1);
  send(nullptr);
}

void setup() {
  // Communication with the computer
  Serial.begin(115200);
  // Communication with the ESP
  Serial1.begin(115200);

  connectWifi();
  connectRelay();

  task_t task = { .delay = 2000, .lastRun = millis(), .loop = true, .task = send, .context = nullptr };
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

  task.loop = false;
  task.task = setupBins;
  task.delay = 0;
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Setup bins task not added");
#endif
  }

  task.loop = true;
  task.task = getChar;
  task.delay = 100;
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Get character from keypad task not added");
#endif
  }

  task.task = readRFID;
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Read card from RFID task not added");
#endif
  }

  task.task = readObstacleSensor;
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Read obstacle sensor task not added");
#endif
  }

  task.task = readFlameSensor;
  task.delay = 500;
  if (!scheduler.addTask(task)) {
#ifdef DEBUG
    Serial.println("Read flame sensor task not added");
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