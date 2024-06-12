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
  while (this->serial->available() > 0) {  // Data from the ESP-01 to the computer
    String request = this->serial->readString();

    // Serial.println("before");
    // Serial.println(request);
    // Serial.println("after");

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
        Serial.println("Received data");
        for (int i(0); i < data[0]; i++) {
          // TODO: handle this
          Serial.print("Received data ");
          Serial.println(i);
          switch (data[offset++]) {
            case trash_0_lid_a:
              bool openLid0 = trashLidA(data, ++offset);
              Serial.print("Trash 0 lid action: ");
              Serial.println(openLid0 ? "open" : "close");
              break;
            case trash_2_lid_a:
              bool openLid2 = trashLidA(data, ++offset);
              Serial.print("Trash 2 lid action: ");
              Serial.println(openLid2 ? "open" : "close");
              break;
            case trash_1_buzzer:
              int music = trashBuzzer(data, ++offset);
              Serial.print("Trash 1 music to play: ");
              Serial.println(music);
              break;
            case trash_2_display:
              size_t size(0);
              unsigned char* text = trashDisplay(data, size, ++offset);
              Serial.print("Trash 2 text to display(size: ): ");
              Serial.print(size);
              Serial.println(text);
              break;
            case trash_0_request_collect:
              break;
            case trash_1_request_collect:
              break;
            case trash_2_request_collect:
              break;
            case simulation_a:
              break;
            default:
              Serial.println("unknown action");
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