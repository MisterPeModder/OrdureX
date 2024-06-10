#ifndef AT_H
#define AT_H

#include "Arduino.h"

class At {
public:
  At(HardwareSerial& serial);
  ~At();

  void connectRelay();
  void connectWifi();

  void addSendData(const unsigned char* payload, const size_t payloadSize);
  void send();


private:
  HardwareSerial* serial;
  unsigned char statusNumber;
  unsigned char* request;
  int offset;
};

#endif  // AT_H