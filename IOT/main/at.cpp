#include "Print.h"
#include "at.h"
#include "config.h"

#define REQUEST_SIZE 256

#ifdef DEBUG
#define DEBUG_PRINT_REQUEST() \
  { \
    Serial.print("Request size: "); \
    Serial.println(offset); \
    Serial.print("Request content: "); \
    Serial.print(statusNumber); \
    for (int i = 0; i < offset; i++) Serial.print(request[i], HEX); \
    Serial.println(); \
  }
#define DEBUG_PRINT_ADD_REQUEST() Serial.println("Adding a request");
#define DEBUG_PRINT_SEND_NOTHING() Serial.println("Nothing to send");
#define DEBUG_PRINT_SEND_REQUEST() \
  { \
    Serial.print("Sending a request. Number of status: "); \
    Serial.println(statusNumber); \
  }
#else
#define DEBUG_PRINT_REQUEST()
#define DEBUG_PRINT_ADD_REQUEST()
#define DEBUG_PRINT_SEND_NOTHING()
#define DEBUG_PRINT_SEND_REQUEST()
#endif

At::At(HardwareSerial& serial)
  : statusNumber(0), offset(0) {
  request = new unsigned char[REQUEST_SIZE]();
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
  DEBUG_PRINT_REQUEST();
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