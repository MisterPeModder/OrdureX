#ifndef AT_H
#define AT_H

#define REQUEST_SIZE 256

#include "Arduino.h"

class At {
public:
  At(HardwareSerial& serial);
  ~At();

  void connectRelay();
  void connectWifi();

  void addSendData(const unsigned char* payload, const size_t payloadSize);
  void send();
  void receive();

private:
  HardwareSerial* serial;
  unsigned char statusNumber;
  unsigned char request[REQUEST_SIZE];
  int offset;
};

#endif  // AT_H