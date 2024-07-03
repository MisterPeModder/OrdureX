#include "bin.h"
#include "actuator.h"
#include "at.h"
#include "binary.h"
#include "config.h"
#include "Keypad.h"
#include "LiquidCrystal.h"
#include "MFRC522.h"
#include "SPI.h"

const unsigned char RFID_UID[RFID_UID_SIZE] = DEFAULT_RFID_UID;
const char password[] = DEFAULT_KEYPAD_PASSWORD;

#define LCD_PASSWORD_OFFSET 10
LiquidCrystal lcd(PIN_DISPLAY_RESET, PIN_DISPLAY_ENABLE, PIN_DISPLAY_D4, PIN_DISPLAY_D4 + 1, PIN_DISPLAY_D4 + 2, PIN_DISPLAY_D4 + 3);
uint8_t lcdCursor(LCD_PASSWORD_OFFSET);

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
#define DEBUG_PRINT_INPUT_REQUEST_COLLECT() \
  Serial.println("Requesting collect of bin 2"); \
  DEBUG_PRINT_INPUT();
#define DEBUG_PRINT_INPUT_WRONG() Serial.println("Wrong password!");
#define DEBUG_PRINT_SHOW_INPUT() \
  Serial.println("Request to show password!"); \
  DEBUG_PRINT_INPUT();
#define DEBUG_PRINT_SHOW_STARS() \
  { \
    Serial.print("Password: "); \
    for (int i = 0; i < passwordOffset; i++) Serial.print('*'); \
    Serial.println(); \
  }
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
  if (lastKey == '\0' && keypad.getState() == KeyState::RELEASED) {
    lcd.setCursor(0, 1);
    lcd.print("                ");
    for (int i = 0; i < passwordOffset; i++) {
      lcd.setCursor(LCD_PASSWORD_OFFSET + i, 0);
      lcd.print('*');
    }
  }
  if (lastKey == '*' && keypad.getState() == KeyState::HOLD) {
    DEBUG_PRINT_SHOW_INPUT();
    lastKey = '\0';

    lcd.setCursor(0, 1);
    lcd.print("Display password");
    for (int i = 0; i < passwordOffset; i++) {
      lcd.setCursor(LCD_PASSWORD_OFFSET + i, 0);
      lcd.print(input[i]);
    }

    return;  // if holding, keypad will not notice any key, returning
  }

  const char key = keypad.getKey();  // Read the key
  if (key == '\0')
    return;
  lastKey = key;

  switch (key) {
    case '*':
      break;
    case '#':
      DEBUG_PRINT_INPUT_RESET();
      passwordOffset = 0;
      lcdCursor = LCD_PASSWORD_OFFSET;

      lcd.setCursor(0, 1);
      lcd.print("Reset password");
      for (int i = 0; i <= PASSWORD_SIZE; i++) {
        lcd.setCursor(LCD_PASSWORD_OFFSET + i, 0);
        lcd.print(" ");
      }
      break;
    default:
      if (passwordOffset <= PASSWORD_SIZE - 1) {
        input[passwordOffset++] = key;

        lcd.setCursor(lcdCursor++, 0);
        lcd.print('*');
        lcd.setCursor(0, 1);
        lcd.print("                ");
      }
      DEBUG_PRINT_SHOW_STARS();

      if (passwordOffset >= PASSWORD_SIZE) {
        if (strcmp(password, input) != 0) {
          DEBUG_PRINT_INPUT_WRONG();
          lcd.setCursor(0, 1);
          lcd.print("Wrong password  ");
        } else {
          DEBUG_PRINT_INPUT_REQUEST_COLLECT();
          addSendData(trash2CollectRequested(), 1);
          lcd.setCursor(0, 1);
          lcd.print("Good password! ");
        }

        lcdCursor = LCD_PASSWORD_OFFSET;
        for (int i = 0; i <= PASSWORD_SIZE; i++) {
          lcd.setCursor(LCD_PASSWORD_OFFSET + i, 0);
          lcd.print(" ");
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
        buzzerGood();
      } else {
        DEBUG_PRINT_RFID_FAILURE();
        buzzerWrong();
      }
      rfid.PICC_HaltA();       // halt PICC
      rfid.PCD_StopCrypto1();  // stop encryption on PCD
    }
  }
}

//------------------ Flame sensor ------------------

#ifdef DEBUG
#define DEBUG_PRINT_FLAME() Serial.println("Bin 1 is burning!");
#define DEBUG_PRINT_NOT_BURNING() Serial.println("Bin 1 is not burning!");
#else
#define DEBUG_PRINT_FLAME()
#define DEBUG_PRINT_NOT_BURNING()
#endif

void readFlameSensor(void *) {
  static bool burning = false;

  if (digitalRead(PIN_FIRE_DIGITAL) == HIGH) {
    DEBUG_PRINT_FLAME();

    burning = true;
    addSendData(trash1Burning(true), 2);
    buzzerFire();
  } else {
    // if bin was burning, send stop burning once
    if (burning) {
      DEBUG_PRINT_NOT_BURNING();
      burning = false;
      addSendData(trash1Burning(false), 2);
    }
  }
}

//------------------ Obstacle sensor ------------------

#ifdef DEBUG
#define DEBUG_PRINT_OBSTACLE() Serial.println("Something is in front bin 0!");
#define DEBUG_PRINT_CLOSE_0() Serial.println("Closing lid of bin 0!");
#else
#define DEBUG_PRINT_OBSTACLE()
#define DEBUG_PRINT_CLOSE_0()
#endif

void readObstacleSensor(void *) {
  static bool opened = false;
  static unsigned long openingStemp = 0;

  if (digitalRead(PIN_OBSTACLE) == LOW) {
    // detect obstacle once
    if (!opened) {
      DEBUG_PRINT_OBSTACLE();
      addSendData(trash0LidS(true), 2);
      lidOpen(0);
    }

    opened = true;
    openingStemp = millis();
  } else {
    // close lid after a certain delay
    if (opened && digitalRead(PIN_OBSTACLE) == HIGH && millis() > (openingStemp + OBSTACLE_DELAY * 1000)) {
      DEBUG_PRINT_CLOSE_0();
      opened = false;
      addSendData(trash0LidS(false), 2);
      lidClose(0);
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

  pinMode(PIN_FIRE_DIGITAL, INPUT);
  pinMode(PIN_OBSTACLE, INPUT_PULLUP);

  // set up the LCD's number of columns and rows:
  lcd.begin(16, 2);
  // print a message to the LCD.
  lcd.print("Password: ");

  setupActuators();
}