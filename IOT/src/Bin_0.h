#ifndef BIN_0_H

/**
 * Calculate the distance of an object from the sensor
 * @param duration: the time it takes for the sound to bounce back
 */
int CalculateDistance(int duration) {
  return (duration*.0343)/2;
}

#define BIN_0_H
#endif // BIN_0_H
