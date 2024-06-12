#include <gtest/gtest.h>

#include <iostream>
#include "../binary.h"

// For reference on assertions, see:
// See https://google.github.io/googletest/reference/assertions.html

//------------------ Status ------------------

TEST(BinaryTests, trash0CollectRequested) {
  unsigned char* data = trash0CollectRequested();
  EXPECT_EQ(data[0], 0x00);
}

TEST(BinaryTests, trash1CollectRequested) {
  unsigned char* data = trash1CollectRequested();
  EXPECT_EQ(data[0], 0x01);
}

TEST(BinaryTests, trash2CollectRequested) {
  unsigned char* data = trash2CollectRequested();
  EXPECT_EQ(data[0], 0x02);
}

TEST(BinaryTests, trash0InvalidCode) {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = trash0InvalidCode(clientId);
  EXPECT_EQ(data[0], 0x03);
  for (int i = 0; i < 16; i++) {
    EXPECT_EQ(data[i + 1], clientId[i]);
  }
}

TEST(BinaryTests, trash1invalidCode) {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = trash1InvalidCode(clientId);
  EXPECT_EQ(data[0], 0x04);
  for (int i = 0; i < 16; i++) {
    EXPECT_EQ(data[i + 1], clientId[i]);
  }
}

TEST(BinaryTests, trash2invalidCode) {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = trash2InvalidCode(clientId);
  EXPECT_EQ(data[0], 0x05);
  for (int i = 0; i < 16; i++) {
    EXPECT_EQ(data[i + 1], clientId[i]);
  }
}

TEST(BinaryTests, trash1Burning) {
  unsigned char* data = trash1Burning();
  EXPECT_EQ(data[0], 0x06);
}

TEST(BinaryTests, trash0LidS_open) {
  unsigned char* data = trash0LidS(true);
  EXPECT_EQ(data[0], 0x07);
  EXPECT_EQ(data[1], 0x01);
}

TEST(BinaryTests, trash0LidS_close) {
  unsigned char* data = trash0LidS(false);
  EXPECT_EQ(data[0], 0x07);
  EXPECT_EQ(data[1], 0x00);
}

TEST(BinaryTests, trash2LidS_open) {
  unsigned char* data = trash2LidS(true);
  EXPECT_EQ(data[0], 0x08);
  EXPECT_EQ(data[1], 0x01);
}

TEST(BinaryTests, trash2LidS_close) {
  unsigned char* data = trash2LidS(false);
  EXPECT_EQ(data[0], 0x08);
  EXPECT_EQ(data[1], 0x00);
}

TEST(BinaryTests, simulationS) {
  unsigned char clientId[16] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                                 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
  unsigned char* data = simulationS(true, clientId);
  EXPECT_EQ(data[0], 0x09);
  EXPECT_EQ(data[1], 0x01);
  for (int i = 0; i < 16; i++) {
    EXPECT_EQ(data[i + 2], clientId[i]);
  }
}

//------------------ Raw actions ------------------

#define OFFSET_START 0

TEST(BinaryTests, getActionType) {
  unsigned char action[1] = { 0x00 };
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_0_lid_a);

  action[0] = 0x01;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_2_lid_a);

  action[0] = 0x02;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_1_buzzer);

  action[0] = 0x03;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_2_display);

  action[0] = 0x04;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_0_request_collect);

  action[0] = 0x05;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_1_request_collect);

  action[0] = 0x06;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::trash_2_request_collect);

  action[0] = 0x07;
  EXPECT_EQ(getActionType(action, OFFSET_START), TopicAction::simulation_a);
}

TEST(BinaryTests, trashLidA) {
  // open
  unsigned char action[2] = { TopicAction::trash_0_lid_a, 0x01 };
  EXPECT_TRUE(trashLidA(action, OFFSET_START));

  // close
  action[1] = 0x00;
  EXPECT_FALSE(trashLidA(action, OFFSET_START));
}

TEST(BinaryTests, trashBuzzer) {
  unsigned char action[2] = { TopicAction::trash_1_buzzer, 0x00 };
  EXPECT_EQ(trashBuzzer(action, OFFSET_START), 0x00);
}

TEST(BinaryTests, trashDisplay) {
  unsigned char action[10] = { TopicAction::trash_2_display, 0x00, 0x80 }; // 0x80 will be overwritten
  size_t size(0);

  unsigned char* value = trashDisplay(action, OFFSET_START, size);
  EXPECT_EQ(size, 0x00);

  // string of 5 characters
  action[1] = 0x05;
  // content
  action[2] = 0x31;
  action[3] = 0x32;
  action[4] = 0x33;
  action[5] = 0x34;
  action[6] = 0x35;

  value = trashDisplay(action, OFFSET_START, size);
  EXPECT_EQ(size, 0x05);
  std::cout << "Values requested to display(size: " << size << "): ";
  for (size_t i = 0; i < size; i++) {
    EXPECT_EQ(value[i], action[i+2]);
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

  value = trashDisplay(action, OFFSET_START, size);
  EXPECT_EQ(size, 0x08);
  std::cout << "Values requested to display(size: " << size << "): ";
  for (size_t i = 0; i < size; i++) {
    EXPECT_EQ(value[i], action[i+2]);
    std::cout << value[i];
  }
  std::cout << std::endl;
}

TEST(BinaryTests, trashRequestCollect) {
  unsigned char action[30] = { TopicAction::trash_0_request_collect,
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

  code = trashRequestCollect(action, OFFSET_START, clientId, size);

  // verify client id
  std::cout << "Client id that requested trash collect: ";
  for (int i = 0; i < 16; i++) {
    EXPECT_EQ(clientId[i], action[i + 1]);
    std::cout << clientId[i];
  }
  // verify code
  EXPECT_EQ(size, 0x03);
  std::cout << ", associated code (size: " << size << "): ";
  for (size_t i = 0; i < size; i++) {
    EXPECT_EQ(code[i], action[i + 18]);
    std::cout << code[i];
  }
  std::cout << std::endl;
}

TEST(BinaryTests, simulationA) {
  unsigned char action[30] = { TopicAction::trash_0_request_collect,
                                SimulationAction::simulation_stop,
                                // client id
                                0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C,
                                0x4D, 0x4E, 0x4F, 0x50 };
  unsigned char clientId[16];
  SimulationAction simAction;
  
  // stop requested
  simAction = simulationA(action, OFFSET_START, clientId);
  EXPECT_EQ(simAction, 0x00);
  std::cout << "Action on the simulation requested: " << (int)simAction << ", client id: ";
  for (size_t i = 0; i < 16; i++) {
    EXPECT_EQ(clientId[i], action[i + 2]);
    std::cout << clientId[i];
  }
  std::cout << std::endl;

  
  action[1] = SimulationAction::simulation_launch;
  action[2] = 0x00;
  simAction = simulationA(action, OFFSET_START, clientId);
  EXPECT_EQ(simAction, 0x01);
  EXPECT_NE(clientId[0], 0x00);// testing if client id is not overwritten
  std::cout << "Action on the simulation requested: " << (int)simAction << ", no client id" << std::endl;
}

//------------------ Several actions in a request ------------------

TEST(BinaryTests, severalActions) {
  int offset(1), i(0);
  size_t size(0);
  unsigned char clientId[16];
  unsigned char* value;
  // open
  unsigned char actions[100] = {
    // 3 actions to handle
    0x03,
    // first action
    TopicAction::trash_0_lid_a, 0x01,
    // second action
    TopicAction::trash_2_display, 0x05, 0x31, 0x32, 0x33, 0x34, 0x35,

    // third action
    TopicAction::trash_0_request_collect,
    // client id
    0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
    0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C,
    0x4D, 0x4E, 0x4F, 0x50,
    // code length
    0x03,
    // code content
    0x31, 0x32, 0x33
  };

  for (int j = 0; j < actions[0]; j++) {
    switch (actions[offset]) {
      case trash_0_lid_a:
        // first action handled
        EXPECT_TRUE(trashLidA(actions, offset));
        // skip 2 bytes
        offset += 2;
        break;
      case trash_2_display:
        // second action handled
        value = trashDisplay(actions, offset, size);
        EXPECT_EQ(size, 0x05);
        for (i = 0; i < (int)size; i++) {
          EXPECT_EQ(value[i], actions[offset+i+2]);
        }
        delete value;
        offset += size + 2;
        break;
      case trash_0_request_collect:
        // third action handled
        value = trashRequestCollect(actions, offset, clientId, size);

        // verify client id
        for (i = 0; i < 16; i++) {
          EXPECT_EQ(clientId[i], actions[offset + i + 1]);
        }
        // verify code
        EXPECT_EQ(size, 0x03);
        for (i = 0; i < (int)size; i++) {
          EXPECT_EQ(value[i], actions[offset + i + 18]);
        }
        break;
    
      default:
        FAIL() << "Wrong action type: " << (int)actions[offset] << " at offset: " << offset << " in the request";
        break;
    }
  }
}