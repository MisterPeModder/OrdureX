#ifndef AT_H
#define AT_H

#include "Arduino.h"

/**
 * Status sent by the Arduino
 */
enum topicStatus {
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
enum topicAction {
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

  void addSendData(topicStatus topic, const byte* payload, const size_t payloadSize);
  void send();


  void dummySend() {
    // byte status_simulation[] PROGMEM = {
    //   0x01, 0x09, 0x01, 0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7,
    //   0x98, 0x91, 0x47
    // };

    // array size is 35
    byte invalid_code[] = {
      0x02, 0x03, 0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7, 0x98,
      0x91, 0x47, 0x05, 0x36, 0x73, 0x98, 0x9e, 0x8c, 0x85, 0x42, 0x71, 0x86, 0x85, 0x8a, 0xea, 0xc7,
      0x98, 0x91, 0x47
    };

    this->serial->println();
    this->serial->print("AT+CIPSEND=");
    this->serial->println(35);
    delay(100);
    this->serial->write(invalid_code, 35);
    Serial.write(invalid_code, 35);
  }

private:
  HardwareSerial* serial;
  byte* request;
  int requestSize;
};

#endif  // AT_H