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

/**
 * @brief Read flame sensor
 */
void readFlameSensor(void *);

/**
 * @brief Read obstacle sensor
 */
void readObstacleSensor(void *);

#endif  // BIN_H