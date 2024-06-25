#ifndef BIN_H
#define BIN_H

/**
 * @brief Global setup for bins' actuators and sensors
 */
void setupBins(void *);

/**
 * @brief Get character from keypad
 */
void getChar(void *);

/**
 * @brief Read card from RFID
 */
void readRFID(void *);

#endif  // BIN_H