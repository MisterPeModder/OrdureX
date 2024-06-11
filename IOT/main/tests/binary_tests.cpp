#include <cassert>
#include <iostream>
#include "../binary.h"

//------------------ Status ------------------

void test_trash0CollectRequested() {
  unsigned char* data = trash0CollectRequested();
  assert(data[0] == 0x00);
}

void test_trash1CollectRequested() {
  unsigned char* data = trash1CollectRequested();
  assert(data[0] == 0x01);
}

void test_trash2CollectRequested() {
  unsigned char* data = trash2CollectRequested();
  assert(data[0] == 0x02);
}

void test_trash0InvalidCode() {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = trash0InvalidCode(clientId);
  assert(data[0] == 0x03);
  for (int i = 0; i < 16; i++) {
    assert(data[i + 1] == clientId[i]);
  }
}

void test_trash1invalidCode() {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = trash1InvalidCode(clientId);
  assert(data[0] == 0x04);
  for (int i = 0; i < 16; i++) {
    assert(data[i + 1] == clientId[i]);
  }
}

void test_trash2invalidCode() {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = trash2InvalidCode(clientId);
  assert(data[0] == 0x05);
  for (int i = 0; i < 16; i++) {
    assert(data[i + 1] == clientId[i]);
  }
}

void test_trash1Burning() {
  unsigned char* data = trash1Burning();
  assert(data[0] == 0x06);
}

void test_trash0LidS_open() {
  unsigned char* data = trash0LidS(true);
  assert(data[0] == 0x07);
  assert(data[1] == 0x01);
}

void test_trash0LidS_close() {
  unsigned char* data = trash0LidS(false);
  assert(data[0] == 0x07);
  assert(data[1] == 0x00);
}

void test_trash2LidS_open() {
  unsigned char* data = trash2LidS(true);
  assert(data[0] == 0x08);
  assert(data[1] == 0x01);
}

void test_trash2LidS_close() {
  unsigned char* data = trash2LidS(false);
  assert(data[0] == 0x08);
  assert(data[1] == 0x00);
}

void test_simulationS() {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = simulationS(true, clientId);
  assert(data[0] == 0x09);
  assert(data[1] == 0x01);
  for (int i = 0; i < 16; i++) {
    assert(data[i + 2] == clientId[i]);
  }
}

//------------------ Actions ------------------

void test_getActionType() {
  unsigned char action[1] = { 0x00 };
  assert(getActionType(action) == TopicAction::trash_0_lid_a);

  action[0] = 0x01;
  assert(getActionType(action) == TopicAction::trash_2_lid_a);

  action[0] = 0x02;
  assert(getActionType(action) == TopicAction::trash_1_buzzer);

  action[0] = 0x03;
  assert(getActionType(action) == TopicAction::trash_2_display);

  action[0] = 0x04;
  assert(getActionType(action) == TopicAction::trash_0_request_collect);

  action[0] = 0x05;
  assert(getActionType(action) == TopicAction::trash_1_request_collect);

  action[0] = 0x06;
  assert(getActionType(action) == TopicAction::trash_2_request_collect);

  action[0] = 0x07;
  assert(getActionType(action) == TopicAction::simulation_a);
}

void test_trashLidA() {
  // open
  unsigned char action[2] = { TopicAction::trash_0_lid_a, 0x01 };
  assert(trashLidA(action));

  // close
  action[1] = 0x00;
  assert(!trashLidA(action));
}

void test_trashBuzzer() {
  unsigned char action[2] = { TopicAction::trash_1_buzzer, 0x00 };
  assert(trashBuzzer(action) == 0x00);
}

void test_trashDisplay() {
  unsigned char action[10] = { TopicAction::trash_2_display, 0x00, 80 };
  size_t size(0);

  unsigned char* value = trashDisplay(action, size);
  assert(size == 0x00);

  // string of 5 characters
  action[1] = 0x05;
  // content
  action[2] = 0x31;
  action[3] = 0x32;
  action[4] = 0x33;
  action[5] = 0x34;
  action[6] = 0x35;

  value = trashDisplay(action, size);
  assert(size == 0x05);
  std::cout << "Values requested to display: ";
  for (size_t i = 0; i < size; i++) {
    assert(value[i] == action[i+2]);
    std::cout << value[i];
  }
  std::cout << std::endl;
  delete value;

  // string of 8 characters
  action[1] = 0x08;
  // content
  action[2] = 0x41;
  action[3] = 0x42;
  action[4] = 0x43;
  action[5] = 0x44;
  action[6] = 0x45;
  action[7] = 0x46;
  action[8] = 0x47;
  action[9] = 0x48;

  value = trashDisplay(action, size);
  assert(size == 0x08);
  std::cout << "Values requested to display: ";
  for (size_t i = 0; i < size; i++) {
    assert(value[i] == action[i+2]);
    std::cout << value[i];
  }
  std::cout << std::endl;
}

void test_trashRequestCollect() {
  unsigned char action[21] = { TopicAction::trash_0_request_collect,
                                // client id
                                0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C,
                                0x4D, 0x4E, 0x4F, 0x50,
                                // code length
                                0x03,
                                // code content
                                0x31, 0x32, 0x33 };
  unsigned char clientId[16];
  unsigned char* code;
  size_t size(0);

  code = trashRequestCollect(action, clientId, size);

  // verify client id
  std::cout << "Client id that requested trash collect: ";
  for (int i = 0; i < 16; i++)
  {
    assert(clientId[i] == action[i + 1]);
    std::cout << clientId[i];
  }
  // verify code
  assert(size == 0x03);
  std::cout << ", associated code (size: ";
  std::cout << size;
  std::cout << "): ";
  for (size_t i = 0; i < size; i++)
  {
    assert(code[i] == action[i + 18]);
    std::cout << code[i];
  }
  std::cout << std::endl;
}

//------------------ Main ------------------

int main() {
  std::cout << "Running status tests..." << std::endl;

  test_trash0CollectRequested();
  test_trash1CollectRequested();
  test_trash2CollectRequested();

  test_trash0InvalidCode();
  test_trash1invalidCode();
  test_trash2invalidCode();

  test_trash1Burning();
  test_trash0LidS_open();
  test_trash0LidS_close();
  test_trash2LidS_open();
  test_trash2LidS_close();

  test_simulationS();

  std::cout << "Running actions tests..." << std::endl;

  test_getActionType();

  test_trashLidA();
  test_trashBuzzer();
  test_trashDisplay();

  test_trashRequestCollect();

  std::cout << "All tests passed!" << std::endl;

  return 0;
}