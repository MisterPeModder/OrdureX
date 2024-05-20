const String ssid = "";
const String password = "";


void setup() {
  Serial.begin(9600); // Communication with the computer
  Serial1.begin(115200); // Communication with the ESP-01

  // Disable the Mega's microcontroller to avoid interference
  pinMode(10, OUTPUT);
  digitalWrite(10, HIGH); // Pull the pin high to disable the Mega's microcontroller

  Serial1.println("AT+RESTORE");
  delay(1000);
  Serial1.println("AT+CWMODE=1");
  delay(1000);
  String wifiConnexion = "AT+CWJAP=\"" + ssid + "\",\"" + password + "\"";
  Serial1.println(wifiConnexion);
  delay(5000);
}

void loop() {
  if (Serial.available()) { // Data from the computer to the ESP-01
    Serial1.write(Serial.read());
  }
  if (Serial1.available()) { // Data from the ESP-01 to the computer
    Serial.write(Serial1.read());
  }
}