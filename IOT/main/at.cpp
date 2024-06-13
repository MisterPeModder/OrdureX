#include "Print.h"
#include "at.h"
#include "binary.h"
#include "config.h"


#ifdef DEBUG
#define DEBUG_PRINT_REQUEST() \
  { \
    Serial.print("Request size: "); \
    Serial.println(offset); \
    Serial.print("Request content: "); \
    Serial.print(statusNumber); \
    for (int i(0); i < offset; i++) Serial.print(request[i], HEX); \
    Serial.println(); \
  }
#define DEBUG_PRINT_ADD_REQUEST() Serial.println("Adding a request");
#define DEBUG_PRINT_RECEIVE(index, data, size) \
  { \
    Serial.print("Data received. Length: "); \
    Serial.print(index); \
    Serial.print(", content: "); \
    for (int i(0); i < size; i++) Serial.print(data[i], HEX); \
    Serial.println(); \
  }
#define DEBUG_PRINT_SEND_NOTHING() Serial.println("Nothing to send");
#define DEBUG_PRINT_SEND_REQUEST() \
  { \
    Serial.print("Sending a request. Number of status: "); \
    Serial.println(statusNumber); \
  }
#else
#define DEBUG_PRINT_REQUEST()
#define DEBUG_PRINT_ADD_REQUEST()
#define DEBUG_PRINT_RECEIVE(index, data, size)
#define DEBUG_PRINT_SEND_NOTHING()
#define DEBUG_PRINT_SEND_REQUEST()
#endif

At::At(HardwareSerial& serial)
  : statusNumber(0), offset(0) {
  this->serial = &serial;
}

void At::connectRelay() {
  this->serial->print("AT+CIPSTART=\"TCP\",\"");
  this->serial->print(RELAY_HOST);
  this->serial->print("\",");
  this->serial->println(RELAY_PORT);

  delay(2000);
}

void At::connectWifi() {
  // disable AT commands echoing
  this->serial->println("ATE0");
  delay(500);
  // wifi client mode
  this->serial->println("AT+CWMODE=1");
  delay(500);

  this->serial->print("AT+CWJAP=\"");
  this->serial->print(WIFI_SSID);
  this->serial->print("\",\"");
  this->serial->print(WIFI_PASSWORD);
  this->serial->println("\"");
  delay(5000);
}

void At::addSendData(const unsigned char* payload, const size_t payloadSize) {
  memcpy(request + offset, payload, payloadSize);

  offset += payloadSize;
  statusNumber++;

  DEBUG_PRINT_ADD_REQUEST();
  //DEBUG_PRINT_REQUEST();
}

void At::send() {
  if (statusNumber == 0) {
    DEBUG_PRINT_SEND_NOTHING();
    return;
  }

  this->serial->print("AT+CIPSEND=");
  this->serial->println(offset + 1);
  delay(200);

  this->serial->write(statusNumber);
  this->serial->write(request, offset);

  DEBUG_PRINT_SEND_REQUEST();
  DEBUG_PRINT_REQUEST();

  statusNumber = 0;
  offset = 0;
  // may be useless to reset the array
  memset(request, 0x00, REQUEST_SIZE);
}

void At::receive() {
  while (this->serial->available() > 0) {  // Data from the ESP-01 to the Arduino
    String request = this->serial->readString();

    int index(0);

    // while loop because there may be several responses in the string
    while ((index = request.indexOf("+IPD,", index)) >= 0) {
      // this is ugly, length must be 2 characters max
      int size(request.substring(index + 5, index + 5 + 2).toInt());
      unsigned char data[size];
      request.substring(index + 7, index + 7 + size).getBytes(data, size);
      DEBUG_PRINT_RECEIVE(size, data, size);

      // data is valid
      if (size > 1) {
        int offset(1);
        for (int i(0); i < getActionType(data, offset); i++) {
          #ifdef DEBUG
          Serial.print("Received data ");
          Serial.println(i);
          #endif

          switch (data[offset++]) {
            case trash_0_lid_a:
              bool openLid0 = trashLidA(data, offset);
              #ifdef DEBUG
              Serial.print("Trash 0 lid action: ");
              Serial.println(openLid0 ? "open" : "close");
              #endif
              offset += 2;

              // handle here
              break;
            case trash_2_lid_a:
              bool openLid2 = trashLidA(data, offset);
              #ifdef DEBUG
              Serial.print("Trash 2 lid action: ");
              Serial.println(openLid2 ? "open" : "close");
              #endif
              offset += 2;

              // handle here
              break;
            case trash_1_buzzer:
              int music = trashBuzzer(data, offset);
              #ifdef DEBUG
              Serial.print("Trash 1 music to play: ");
              Serial.println(music);
              #endif
              offset += 2;

              // handle here
              break;
            case trash_2_display:
              size_t size(0); // size of string
              unsigned char* text = trashDisplay(data, offset, size);
              #ifdef DEBUG
              Serial.print("Trash 2 text to display(size: ");
              Serial.print(size);
              Serial.print("): ");
              //Serial.println(text);
              #endif
              offset += 2 + size;

              // handle here
              break;
            case trash_0_request_collect:
              unsigned char clientId[16]; // UUID received
              unsigned char* code = trashRequestCollect(data, offset, clientId, size);
              #ifdef DEBUG
              Serial.print("Trash 0 request collect: ");
              for (int i(0); i < size; i++) {
                Serial.print(code[i], HEX);
              }
              Serial.println();
              #endif
              offset += 16 + 1 + 1 + size; // UUID + type + length

              // handle here

              delete[] code;
              break;
            case trash_1_request_collect:
              code = trashRequestCollect(data, offset, clientId, size);
              #ifdef DEBUG
              Serial.print("Trash 1 request collect: ");
              for (int i(0); i < size; i++) {
                Serial.print(code[i], HEX);
              }
              Serial.println();
              #endif
              offset += 16 + 1 + 1 + size;

              // handle here

              delete[] code;
              break;
            case trash_2_request_collect:
              code = trashRequestCollect(data, offset, clientId, size);
              #ifdef DEBUG
              Serial.print("Trash 2 request collect: ");
              for (int i(0); i < size; i++) {
                Serial.print(code[i], HEX);
              }
              Serial.println();
              #endif
              offset += 16 + 1 + 1 + size;

              // handle here

              delete[] code;
              break;
            case simulation_a:
              SimulationAction action = simulationA(data, offset, clientId);
              #ifdef DEBUG
              Serial.print("Simulation action: ");
              Serial.println(action);
              #endif
              offset += 1 + 1 + 16;

              break;
            default:
              #ifdef DEBUG
              Serial.println("Unknown action");
              #endif
          }
        }
      }

      // next
      if (index >= 0) {
        index++;
      }
    }
  }
}