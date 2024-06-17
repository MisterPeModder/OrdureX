#ifndef AT_H
#define AT_H


#include "Arduino.h"

void connectRelay();
void connectWifi();

void addSendData(const unsigned char* payload, const size_t payloadSize);
void send(void* context);
void receive(void* context);

#endif  // AT_H