#include "actuator.h"
#include "config.h"
#include "Arduino.h"
#include "Servo.h"

Servo servo0;

void buzzerFire() {
  // simulating a fire alarm
  for (int i = 0; i < 20; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(600);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(600);
  }
  delay(70);
  for (int i = 0; i < 150; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(700);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(700);
  }
}

void buzzerGood() {
  // high pitch sound on success
  for (int i = 0; i < 20; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(1200);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(1200);
  }
  delay(70);
  for (int i = 0; i < 50; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(800);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(800);
  }
}

void buzzerMusic() {
  for (int i = 0; i < 50; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(800);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(800);
  }
  delay(70);
  for (int i = 0; i < 50; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(700);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(700);
  }
  delay(70);
  for (int i = 0; i < 50; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(600);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(600);
  }
  delay(70);
  for (int i = 0; i < 40; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(500);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(500);
  }
  delay(70);
  for (int i = 0; i < 100; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(400);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(400);
  }
}

void buzzerWrong() {
  // low pitch sound on failure
  for (int i = 0; i < 30; i++) {
    digitalWrite(PIN_BUZZER_SOURCE, HIGH);
    delayMicroseconds(2200);
    digitalWrite(PIN_BUZZER_SOURCE, LOW);
    delayMicroseconds(2200);
  }
}

void lidClose(int trash) {
    switch (trash) {
    case 0:
      servo0.write(0);
      break;
    // add other trash here
  }
}

void lidOpen(int trash) {
  switch (trash) {
    case 0:
      servo0.write(SERVO_ANGLE);
      break;
    // add other trash here
  }
}

void setupActuators() {
  pinMode(PIN_BUZZER_SOURCE, OUTPUT);

  servo0.attach(PIN_SERVO_0);
  delay(1);
  lidClose(0);
}