#include "binary.h"

#define STRING_MAX_LENGTH 20

//------------------ Status ------------------

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

unsigned char* trash1Burning(bool burning) {
  static unsigned char data[] = { trash_1_burning, 0x00 };
  data[1] = burning ? 0x01 : 0x00;
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

//------------------ Actions ------------------

TopicAction getActionType(const unsigned char* data, const int& offset) {
  return static_cast<TopicAction>(data[offset]);
}

bool trashLidA(const unsigned char* data, const int& offset) {
  return data[offset + 1] == 0x01;
}

int trashBuzzer(const unsigned char* data, const int& offset) {
  return data[offset + 1];
}

unsigned char* trashDisplay(const unsigned char* data, const int& offset, size_t& size) {
  size = data[offset + 1];
  static unsigned char* value = new unsigned char[STRING_MAX_LENGTH];
  for (size_t i = 0; i < size; i++) {
    value[i] = data[offset + i + 2];  // plus 2: skip action type and string size
  }
  return value;
}

unsigned char* trashRequestCollect(const unsigned char* data, const int& offset, unsigned char* clientId, size_t& size) {
  size_t i(0);

  // client id
  for (; i < 16; i++) {
    clientId[i] = data[offset + i + 1];  // plus 1: skip action type
  }

  // secret code
  size = data[offset + ++i];
  static unsigned char* code = new unsigned char[STRING_MAX_LENGTH];
  for (size_t j = 0; j < size; j++) {
    code[j] = data[offset + i + j + 1];
  }

  return code;
}

SimulationAction simulationA(const unsigned char* data, const int& offset, unsigned char* clientId) {
  SimulationAction action(static_cast<SimulationAction>(data[offset + 1]));

  if (action == SimulationAction::simulation_stop) {
    for (int i = 0; i < 16; i++) {
      clientId[i] = data[offset + i + 2];
    }
  }

  return action;
}
