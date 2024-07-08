#include "Arduino.h"
#include "at.h"
#include "actuator.h"
#include "binary.h"
#include "config.h"
#include "scheduler.h"

#define REQUEST_SIZE 256
#define RECEIVE_SIZE 256

#ifdef DEBUG
#define DEBUG_PRINT_REQUEST() \
  { \
    Serial.print("Request size: "); \
    Serial.println(OFFSET); \
    Serial.print("Request content: "); \
    Serial.print(statusNumber); \
    for (int i(0); i < OFFSET; i++) Serial.print(REQUEST[i], HEX); \
    Serial.println(); \
  }
#define DEBUG_PRINT_ADD_REQUEST() Serial.println("Adding a request");
#define DEBUG_PRINT_RECEIVE(index, data, size) \
  { \
    Serial.print("Data received. Length: "); \
    Serial.print(size); \
    Serial.print(", content: "); \
    for (int i(0); i < size; i++) { \
      Serial.print(data[i + index], HEX); \
      Serial.print(' '); \
    } \
    Serial.println(); \
  }
#define DEBUG_PRINT_SEND_NOTHING() Serial.println("Nothing to send");
#define DEBUG_PRINT_SEND_REQUEST() \
  { \
    Serial.print("Sending a request. Number of status: "); \
    Serial.println(statusNumber); \
  }
// specific to reveived data
#define DEBUG_PRINT_RECEIVE_LID(trash, state) \
  { \
    Serial.print("Trash "); \
    Serial.print(trash); \
    Serial.print(" lid action: "); \
    Serial.println(state ? "open" : "close"); \
  }
#define DEBUG_PRINT_RECEIVE_MUSIC(trash, music) \
  { \
    Serial.print("Trash "); \
    Serial.print(trash); \
    Serial.print(" music to play: "); \
    Serial.println(music); \
  }
#define DEBUG_PRINT_RECEIVE_DISPLAY(trash, text, size) \
  { \
    Serial.print("Trash "); \
    Serial.print(trash); \
    Serial.print(" text to display(size: "); \
    Serial.print(size); \
    Serial.print("): "); \
    for (size_t i(0); i < size; i++) { \
      Serial.print((char)text[i]); \
    } \
    Serial.println(); \
  }
#define DEBUG_PRINT_RECEIVE_REQUEST_COLLECT(trash, clientId, code, size) \
  { \
    Serial.print("Trash "); \
    Serial.print(trash); \
    Serial.print(" request collect: client id: "); \
    for (size_t i(0); i < 16; i++) { \
      Serial.print(clientId[i], HEX); \
    } \
    Serial.print(" code(size: "); \
    Serial.print(size); \
    Serial.print("): "); \
    for (size_t i(0); i < size; i++) { \
      Serial.print((char)code[i]); \
    } \
    Serial.println(); \
  }
#else
#define DEBUG_PRINT_REQUEST()
#define DEBUG_PRINT_ADD_REQUEST()
#define DEBUG_PRINT_RECEIVE(index, data, size)
#define DEBUG_PRINT_SEND_NOTHING()
#define DEBUG_PRINT_SEND_REQUEST()
//
#define DEBUG_PRINT_RECEIVE_LID(lid, state)
#define DEBUG_PRINT_RECEIVE_MUSIC(trash, music)
#define DEBUG_PRINT_RECEIVE_DISPLAY(trash, text, size)
#define DEBUG_PRINT_RECEIVE_REQUEST_COLLECT(trash, clientId, code, size)
#endif

unsigned char statusNumber(0);
unsigned char REQUEST[REQUEST_SIZE];
int OFFSET(0);

void connectRelay() {
  Serial1.print("AT+CIPSTART=\"TCP\",\"");
  Serial1.print(RELAY_HOST);
  Serial1.print("\",");
  Serial1.println(RELAY_PORT);

  delay(2000);
}

void connectWifi() {
  // disable AT commands echoing
  Serial1.println("ATE0");
  delay(500);
  // wifi client mode
  Serial1.println("AT+CWMODE=1");
  delay(500);

  Serial1.print("AT+CWJAP=\"");
  Serial1.print(WIFI_SSID);
  Serial1.print("\",\"");
  Serial1.print(WIFI_PASSWORD);
  Serial1.println("\"");
  delay(5000);
}

void addSendData(const unsigned char* payload, const size_t payloadSize) {
  memcpy(REQUEST + OFFSET, payload, payloadSize);

  OFFSET += payloadSize;
  statusNumber++;

  //DEBUG_PRINT_ADD_REQUEST();
  //DEBUG_PRINT_REQUEST();
}

void sendData(void* context) {
  Serial1.write(statusNumber);
  Serial1.write(REQUEST, OFFSET);

  DEBUG_PRINT_SEND_REQUEST();
  DEBUG_PRINT_REQUEST();

  statusNumber = 0;
  OFFSET = 0;
  // may be useless to reset the array
  //memset(REQUEST, 0x00, REQUEST_SIZE);
}

task_t task = { .delay = 200, .lastRun = millis(), .loop = false, .task = sendData, .context = nullptr };

void send(void* context) {
  if (statusNumber == 0) {
    //DEBUG_PRINT_SEND_NOTHING();
    return;
  }

  Serial1.print("AT+CIPSEND=");
  Serial1.println(OFFSET + 1);

  Scheduler* s = static_cast<Scheduler*>(context);
  task.lastRun = millis();
  if(s->addTask(task)) {
    Serial.println("Send data added!");
  } else {
    Serial.println("Send data not added!");
  }
}

void receive(void* context) {
  static unsigned char request[RECEIVE_SIZE];

  while (Serial1.available() > 0) {  // Data from the ESP-01 to the Arduino
    size_t requestLength = Serial1.readBytes(request, RECEIVE_SIZE);
    void* prefix = memmem(request, requestLength, "+IPD,", 5);

    if (prefix != nullptr) {
      size_t dataOffset = (char*)prefix - (char*)request + 5;
      int dataSize = atoi(reinterpret_cast<const char *>(request) + dataOffset);

      if (dataSize > 1) {
        if (dataSize < 10) {
          dataOffset += 2;
        } else if (dataSize >= 10) {
          dataOffset += 3;
        } else if (dataSize >= 100) {
          dataOffset += 4;
        }

        DEBUG_PRINT_RECEIVE(dataOffset, request, dataSize);
        // Index of data
        int index(dataOffset + 1);
        for (int i(0); i < request[dataOffset]; i++) {
#ifdef DEBUG
          Serial.print("Received data ");
          Serial.println(i);
#endif
          switch (request[index]) {
            case TopicAction::trash_0_lid_a:
              {
                bool openLid = trashLidA(request, index);
                DEBUG_PRINT_RECEIVE_LID(0, openLid);
                index += 2;

                if(openLid) {
                  lidOpen(0);
                } else {
                  lidClose(0);
                }
              }
              break;
            case TopicAction::trash_2_lid_a:
              {
                bool openLid = trashLidA(request, index);
                DEBUG_PRINT_RECEIVE_LID(2, openLid);
                index += 2;

                // handle here
              }
              break;
            case TopicAction::trash_1_buzzer:
              {
                int music = trashBuzzer(request, index);
                DEBUG_PRINT_RECEIVE_MUSIC(1, music);
                index += 2;

                buzzerMusic();
              }
              break;
            case TopicAction::trash_2_display:
              {
                size_t size(0);  // size of string
                unsigned char* text = trashDisplay(request, index, size);
                DEBUG_PRINT_RECEIVE_DISPLAY(2, text, size);
                index += 2 + size;

                // handle here
              }
              break;
            case TopicAction::trash_0_request_collect:
              {
                size_t size(0);              // size of string
                unsigned char clientId[16];  // UUID received
                unsigned char* code = trashRequestCollect(request, index, clientId, size);
                DEBUG_PRINT_RECEIVE_REQUEST_COLLECT(0, clientId, code, size);
                index += 16 + 1 + 1 + size;  // UUID + type + length

                // handle here
              }
              break;
            case TopicAction::trash_1_request_collect:
              {
                size_t size(0);              // size of string
                unsigned char clientId[16];  // UUID received
                unsigned char* code = trashRequestCollect(request, index, clientId, size);
                DEBUG_PRINT_RECEIVE_REQUEST_COLLECT(1, clientId, code, size);
                index += 16 + 1 + 1 + size;

                // handle here
              }
              break;
            case TopicAction::trash_2_request_collect:
              {
                size_t size(0);              // size of string
                unsigned char clientId[16];  // UUID received
                unsigned char* code = trashRequestCollect(request, index, clientId, size);
                DEBUG_PRINT_RECEIVE_REQUEST_COLLECT(2, clientId, code, size);
                index += 16 + 1 + 1 + size;

                // handle here
              }
              break;
            case TopicAction::simulation_a:
              {
                unsigned char clientId[16];  // UUID received
                SimulationAction action = simulationA(request, index, clientId);
#ifdef DEBUG
                Serial.print("Simulation action: ");
                Serial.println(action);
#endif
                index += 1 + 1 + 16;

                // handle here
              }
              break;
#ifdef DEBUG
            default:
              Serial.println("Unknown action");
#endif
          }
        }
      }
    }
  }
}

