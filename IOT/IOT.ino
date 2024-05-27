#include "src/Bin_0.h"
#include "Servo.h"
#define pin 2

Servo servo;
int val;

void setup() {
  // put your setup code here, to run once:
  //int i = addOne(1, 2);

  Serial.begin(9600);
  servo.attach(pin);
  //Serial.println(addOne(0));
}

void loop() {
  // put your main code here, to run repeatedly:
  while(Serial.available() > 0) {
    val = Serial.parseInt();
    if(val != 0) {
      Serial.println(val);
      servo.write(val);
    }
  }
}
