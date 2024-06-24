#include "bin.h"
#include "at.h"
#include "binary.h"
#include "config.h"
#include "Keypad.h"

#define PASSWORD_SIZE 4

char password[PASSWORD_SIZE + 1] = "1234";
char input[PASSWORD_SIZE + 1];
int passwordOffset(0);

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
#else
#define DEBUG_PRINT_INPUT()
#endif

char keys[ROWS][COLS] = {
  { '1', '2', '3', 'A' },
  { '4', '5', '6', 'B' },
  { '7', '8', '9', 'C' },
  { '*', '0', '#', 'D' }
};
char lastKey = 0x00;
//connect to the row pinouts of the keypad
byte rowPins[ROWS] = { PIN_KEYPAD_BEGIN + 14, PIN_KEYPAD_BEGIN + 12, PIN_KEYPAD_BEGIN + 10, PIN_KEYPAD_BEGIN + 8 };
//connect to the column pinouts of the keypad
byte colPins[COLS] = { PIN_KEYPAD_BEGIN + 6, PIN_KEYPAD_BEGIN + 4, PIN_KEYPAD_BEGIN + 2, PIN_KEYPAD_BEGIN };

//Create an object of keypad
Keypad keypad = Keypad(makeKeymap(keys), rowPins, colPins, ROWS, COLS);

void getChar(void *) {
  if (lastKey == '*' && keypad.getState() == KeyState::HOLD) {
#ifdef DEBUG
    Serial.println("Request to show password!");
    DEBUG_PRINT_INPUT();
#endif
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
      break;
    case '#':
#ifdef DEBUG
      Serial.println("Reset password");
#endif
      passwordOffset = 0;
      break;
    default:
      if (passwordOffset <= PASSWORD_SIZE - 1) {
        input[passwordOffset++] = key;
        // TODO: show '**...' on display,
      }

      if (passwordOffset >= PASSWORD_SIZE) {
#ifdef DEBUG
        Serial.println("Requesting collect of bin 2");
        DEBUG_PRINT_INPUT();
#endif

        if (strcmp(password, input) != 0) {
#ifdef DEBUG
          Serial.println("Wrong password!");
#endif
        }

        addSendData(trash2CollectRequested(), 1);
        passwordOffset = 0;
      }
  }
}

void setupBins(void *) {
  password[PASSWORD_SIZE] = '\0';
  keypad.setHoldTime(HOLD_TIME);
}