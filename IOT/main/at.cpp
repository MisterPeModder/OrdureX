#include "Print.h"
#include "at.h"
#include "config.h"

At::At(HardwareSerial& serial) {
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
  delay(1000);

  this->serial->print("AT+CWJAP=\"");
  this->serial->print(WIFI_SSID);
  this->serial->print("\",\"");
  this->serial->print(WIFI_PASSWORD);
  this->serial->println("\"");
  delay(5000);
}

void At::addSendData(topicStatus topic, const byte* payload, const size_t payloadSize) {
  if(request == nullptr) {
    request = new byte[1 + payloadSize]();

    request[0] = static_cast<byte>(topic);
    memcpy(request + 1, payload, payloadSize);
    requestSize = payloadSize +1;
  } else {
    // if request already exists, append the new payload
    byte* temp = new byte[requestSize + 1 + payloadSize]();
    memcpy(temp, request, requestSize);
    temp[requestSize + 1] = static_cast<byte>(topic);
    memcpy(temp + 1 + payloadSize, payload, payloadSize);

    delete request;
    *request = *temp;
    requestSize += payloadSize +1;
  }

  // debugging purpose
  for (int i = 0; i < sizeof(request) / sizeof(byte); i++)
    Serial.write(request[i]);
}

void At::send() {
  size_t size = sizeof(request) / sizeof(byte);
  this->serial->print("AT+CIPSEND=");
  this->serial->println(size);

  for (int i = 0; i < size; i++) {
    this->serial->print(request[i], BIN);
    // debugging purpose
    //Serial.print(request[i], BIN);
  }
  
  delete request;
}