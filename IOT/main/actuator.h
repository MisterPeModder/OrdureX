#ifndef ACTUATOR_H
#define ACTUATOR_H

/**
 * @brief Play a fire alarm
 */
void buzzerFire();

/**
 * @brief Play a high pitch sound on the buzzer
 */
void buzzerGood();

/**
 * @brief Play a music
 */
void buzzerMusic();

/**
 * @brief Play a low pitch sound on the buzzer
 */
void buzzerWrong();

/**
 * @brief Close the lid of a trash
 * @param trash The trash number
 */
void lidClose(int trash);
/**
 * @brief Open the lid of a trash
 * @param trash The trash number
 */
void lidOpen(int trash);

/**
 * @brief Setup the actuators
 */
void setupActuators();

#endif  // ACTUATOR_H