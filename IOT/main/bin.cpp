#include "bin.h"
#include "at.h"
#include "binary.h"
#include "config.h"
#include "Keypad.h"
#include "MFRC522.h"
#include "SPI.h"

const unsigned char RFID_UID[RFID_UID_SIZE] = DEFAULT_RFID_UID;
const char password[] = DEFAULT_KEYPAD_PASSWORD;

//------------------ Keypad ------------------

#define ROWS 4          // four rows
#define COLS 4          // four columns
#define HOLD_TIME 1000  // hold time in milliseconds

#ifdef DEBUG
#define DEBUG_PRINT_INPUT() \
  { \
    Serial.print("Password: "); \
    for (int i = 0; i < passwordOffset; i++) Serial.print(input[i]); \
    Serial.println(); \
  }
#define DEBUG_PRINT_INPUT_RESET() Serial.println("Reset password");
#define DEBUG_PRINT_INPUT_REQUEST_COLLECT() Serial.println("Requesting collect of bin 2");DEBUG_PRINT_INPUT();
#define DEBUG_PRINT_INPUT_WRONG() Serial.println("Wrong password!");
#define DEBUG_PRINT_SHOW_INPUT() Serial.println("Request to show password!");DEBUG_PRINT_INPUT();
#define DEBUG_PRINT_SHOW_STARS() Serial.println("Password: ****");
#else
#define DEBUG_PRINT_INPUT()
#define DEBUG_PRINT_INPUT_RESET()
#define DEBUG_PRINT_INPUT_REQUEST_COLLECT()
#define DEBUG_PRINT_INPUT_WRONG()
#define DEBUG_PRINT_SHOW_INPUT()
#define DEBUG_PRINT_SHOW_STARS()
#endif

const char keys[ROWS][COLS] = {
  { '1', '2', '3', 'A' },
  { '4', '5', '6', 'B' },
  { '7', '8', '9', 'C' },
  { '*', '0', '#', 'D' }
};
char lastKey = 0x00;
char input[PASSWORD_SIZE + 1];
int passwordOffset(0);
//connect to the row pinouts of the keypad
byte rowPins[ROWS] = { PIN_KEYPAD_BEGIN + 7, PIN_KEYPAD_BEGIN + 6, PIN_KEYPAD_BEGIN + 5, PIN_KEYPAD_BEGIN + 4 };
//connect to the column pinouts of the keypad
byte colPins[COLS] = { PIN_KEYPAD_BEGIN + 3, PIN_KEYPAD_BEGIN + 2, PIN_KEYPAD_BEGIN + 1, PIN_KEYPAD_BEGIN };

//Create an object of keypad
Keypad keypad = Keypad(makeKeymap(keys), rowPins, colPins, ROWS, COLS);

void getChar(void *) {
  if (lastKey == '*' && keypad.getState() == KeyState::HOLD) {
    DEBUG_PRINT_SHOW_INPUT();
    lastKey = '\0';

    // TODO: show on display,

    return;  // if holding, keypad will not notice any key, returning
  }

  const char key = keypad.getKey();  // Read the key
  if (key == '\0')
    return;
  lastKey = key;

  switch (key) {
    case '*':
      DEBUG_PRINT_SHOW_STARS();
      break;
    case '#':
      DEBUG_PRINT_INPUT_RESET();
      passwordOffset = 0;
      break;
    default:
      if (passwordOffset <= PASSWORD_SIZE - 1) {
        input[passwordOffset++] = key;
        // TODO: show '**...' on display,
      }

      if (passwordOffset >= PASSWORD_SIZE) {
        if (strcmp(password, input) != 0) {
          DEBUG_PRINT_INPUT_WRONG();
        } else {
          DEBUG_PRINT_INPUT_REQUEST_COLLECT();
          addSendData(trash2CollectRequested(), 1);
        }

        passwordOffset = 0;
      }
  }
}

//------------------ RFID ------------------

#ifdef DEBUG
#define DEBUG_PRINT_RFID(uid) \
  { \
    Serial.print("UID from RFID: "); \
    for (int i = 0; i < uid.size; i++) { \
      Serial.print(uid.uidByte[i] < 0x10 ? " 0" : " "); \
      Serial.print(uid.uidByte[i], HEX); \
    } \
    Serial.println(); \
  }
#define DEBUG_PRINT_RFID_SUCCESS() Serial.println("UID matches!");
#define DEBUG_PRINT_RFID_FAILURE() Serial.println("Does not match!");
#else
#define DEBUG_PRINT_RFID(uid)
#define DEBUG_PRINT_RFID_SUCCESS()
#define DEBUG_PRINT_RFID_FAILURE()
#endif

MFRC522 rfid(PIN_RFID_SDA, PIN_RFID_RST);

void readRFID(void *) {
  if (rfid.PICC_IsNewCardPresent()) {  // new tag is available
    if (rfid.PICC_ReadCardSerial()) {  // UID has been read
      DEBUG_PRINT_RFID(rfid.uid);

      if (strncmp(reinterpret_cast<const char *>(rfid.uid.uidByte), reinterpret_cast<const char *>(RFID_UID), RFID_UID_SIZE) == 0) {
        DEBUG_PRINT_RFID_SUCCESS();
        addSendData(trash1CollectRequested(), 1);
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
      } else {
        DEBUG_PRINT_RFID_FAILURE();
        // low pitch sound on failure
        for (int i = 0; i < 30; i++) {
          digitalWrite(PIN_BUZZER_SOURCE, HIGH);
          delayMicroseconds(2200);
          digitalWrite(PIN_BUZZER_SOURCE, LOW);
          delayMicroseconds(2200);
        }
      }
      rfid.PICC_HaltA();       // halt PICC
      rfid.PCD_StopCrypto1();  // stop encryption on PCD
    }
  }
}

//------------------ Flame sensor ------------------

#ifdef DEBUG
#define DEBUG_PRINT_FLAME() Serial.println("Bin 1 is burning!");
#else
#define DEBUG_PRINT_FLAME()
#endif

void readFlameSensor(void *) {
  if (digitalRead(PIN_FIRE_DIGITAL) == HIGH) {
    DEBUG_PRINT_FLAME();
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
}

//------------------ Initial bin configuration ------------------

void setupBins(void *) {
  keypad.setHoldTime(HOLD_TIME);

  SPI.begin();
  rfid.PCD_Init();
#ifdef DEBUG
  Serial.print("RFID module version: ");
  rfid.PCD_DumpVersionToSerial();
#endif

  pinMode(PIN_BUZZER_SOURCE, OUTPUT);
  pinMode(PIN_FIRE_DIGITAL, INPUT);
}