#ifndef BINARY_H
#define BINARY_H

#include <stdbool.h>

#ifndef ARDUINO
#include <stddef.h>
#else
#include "Arduino.h"
#endif

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

/**
 * Simutation actions requested by Android device
 */
enum SimulationAction : unsigned char {
  simulation_stop = 0,
  simulation_launch,
  simulation_pause
};

//------------------ Status ------------------

/**
 * Collect requested on trash 0
 * @return unsigned char* (1 byte)
 */
unsigned char* trash0CollectRequested();

/**
 * Collect requested on trash 1
 * @return unsigned char* (1 byte)
 */
unsigned char* trash1CollectRequested();

/**
 * Collect requested on trash 2
 * @return unsigned char* (1 byte)
 */
unsigned char* trash2CollectRequested();

/**
 * Invalid code on trash 0
 * @param clientId UUID (16 bytes)
 * @return unsigned char* (17 bytes)
 */
unsigned char* trash0InvalidCode(const unsigned char* clientId);

/**
 * Invalid code on trash 1
 * @param clientId UUID (16 bytes)
 * @return unsigned char* (17 bytes)
 */
unsigned char* trash1InvalidCode(const unsigned char* clientId);

/**
 * Invalid code on trash 2
 * @param clientId UUID (16 bytes)
 * @return unsigned char* (17 bytes)
 */
unsigned char* trash2InvalidCode(const unsigned char* clientId);

/**
 * Trash 1 is burning
 * @return unsigned char* (1 byte)
 */
unsigned char* trash1Burning();

/**
 * Trash 0 lid status
 * @param opened true if open, false if closed
 * @return unsigned char* (2 bytes)
 */
unsigned char* trash0LidS(bool opened);

/**
 * Trash 2 lid status
 * @param opened true if open, false if closed
 * @return unsigned char* (2 bytes)
 */
unsigned char* trash2LidS(bool opened);

/**
 * Simulation status
 * @param ready true if ready, false if not ready
 * @param clientId UUID (16 bytes)
 * @return unsigned char* (18 bytes)
 */
unsigned char* simulationS(bool ready, const unsigned char* clientId);

//------------------ Actions ------------------

/**
 * Get the action type
 * @param data binary data of the action
 * @param offset data start offset
 * @return TopicAction
 */
TopicAction getActionType(const unsigned char* data, const int& offset);

/**
 * Trash lid action
 * @param data binary data of the action (2 bytes)
 * @param offset data start offset
 * @return open or close the lid
 */
bool trashLidA(const unsigned char* data, const int& offset);

/**
 * Trash buzzer action
 * @param data binary data of the action (2 byte)
 * @param offset data start offset
 * @return music played by the buzzer
 */
int trashBuzzer(const unsigned char* data, const int& offset);

/**
 * Trash display action
 * @param data binary data of the action (1 + x byte)
 * @param offset data start offset
 * @param size size of the returned string
 * @return string displayed on the screen (/!\ must be deleted)
 */
unsigned char* trashDisplay(const unsigned char* data, const int& offset, size_t& size);

/**
 * Trash request collect action
 * @param data binary data of the action (1 + 16 + x byte)
 * @param offset data start offset
 * @param clientId UUID (16 bytes)
 * @param size size of the returned code
 * @return code to open the trash (x bytes)
 */
unsigned char* trashRequestCollect(const unsigned char* data, const int& offset, unsigned char* clientId, size_t& size);

/**
 * Simulation action
 * @param data binary data of the action (1 + 1 + 16 bytes)
 * @param offset data start offset
 * @param clientId UUID (16 bytes)
 * @return SimulationAction
 */
SimulationAction simulationA(const unsigned char* data, const int& offset, unsigned char* clientId);

#endif  // BINARY_H