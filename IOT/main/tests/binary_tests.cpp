#include <cassert>
#include "../binary.h"

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

int main() {
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

  return 0;
}