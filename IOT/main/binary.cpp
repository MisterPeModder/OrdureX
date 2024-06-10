#include "binary.h"

unsigned char* trash0CollectRequested() {
  static unsigned char data[] = { trash_0_collect_requested };
  return data;
}

unsigned char* trash1CollectRequested() {
  static unsigned char data[] = { trash_1_collect_requested };
  return data;
}

unsigned char* trash2CollectRequested() {
  static unsigned char data[] = { trash_2_collect_requested };
  return data;
}

unsigned char* trash0InvalidCode(const unsigned char* clientId) {
  static unsigned char* data = new unsigned char[17];
  data[0] = trash_0_invalid_code;
  for (int i = 0; i < 16; i++) {
    data[i + 1] = clientId[i];
  }
  return data;
}

unsigned char* trash1InvalidCode(const unsigned char* clientId) {
  static unsigned char* data = new unsigned char[17];
  data[0] = trash_1_invalid_code;
  for (int i = 0; i < 16; i++) {
    data[i + 1] = clientId[i];
  }
  return data;
}

unsigned char* trash2InvalidCode(const unsigned char* clientId) {
  static unsigned char* data = new unsigned char[17];
  data[0] = trash_2_invalid_code;
  for (int i = 0; i < 16; i++) {
    data[i + 1] = clientId[i];
  }
  return data;
}

unsigned char* trash1Burning() {
  static unsigned char data[] = { trash_1_burning };
  return data;
}

unsigned char* trash0LidS(bool opened) {
  static unsigned char data[] = { trash_0_lid_s, 0x00 };
  data[1] = opened ? 0x01 : 0x00;
  return data;
}

unsigned char* trash2LidS(bool opened) {
  static unsigned char data[] = { trash_2_lid_s, 0x00 };
  data[1] = opened ? 0x01 : 0x00;
  return data;
}

unsigned char* simulationS(bool ready, const unsigned char* clientId) {
  static unsigned char* data = new unsigned char[18];
  data[0] = simulation_s;
  data[1] = ready ? 0x01 : 0x00;
  for (int i = 0; i < 16; i++) {
    data[i + 2] = clientId[i];
  }
  return data;
}