#ifndef AT_H
#define AT_H

#include "Arduino.h"

/**
 * Status sent by the Arduino
 */
enum TopicStatus : unsigned char {
  // collect requested
  trash_0_collect_requested = 0,
  trash_1_collect_requested,
  trash_2_collect_requested,
  // invalide code
  trash_0_invalid_code,
  trash_1_invalid_code,
  trash_2_invalid_code,
  // sensors
  trash_1_burning,
  trash_0_lid_s,
  trash_2_lid_s,

  simulation_s
};

/**
 * Requested actions by Android device
 */
enum TopicAction : unsigned char {
  // actuators
  trash_0_lid_a = 0,
  trash_2_lid_a,
  trash_1_buzzer,
  trash_2_display,
  // request collect
  trash_0_request_collect,
  trash_1_request_collect,
  trash_2_request_collect,

  simulation_a
};

class At {
public:
  At(HardwareSerial& serial);
  ~At();

  void connectRelay();
  void connectWifi();

  void addSendData(TopicStatus topic, const byte* payload, const size_t payloadSize);
  void send();


private:
  HardwareSerial* serial;
  unsigned char statusNumber;
  unsigned char* request;
  int offset;
};

#endif  // AT_H