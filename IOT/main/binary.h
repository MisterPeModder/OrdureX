#ifndef BINARY_H
#define BINARY_H

#include <stdbool.h>

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

#endif // BINARY_H