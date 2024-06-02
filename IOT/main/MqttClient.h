#ifndef MQTTCLIENT_H
#define MQTTCLIENT_H

#include "Arduino.h"

class MqttClient {
public:
    MqttClient(HardwareSerial& serial);
    ~MqttClient();

    void connectWifi();
    bool connect();
    void disconnect();

    bool publish(const char* topic, const char* payload, bool retain = false);
    bool subscribe(const char* topic);
    bool unsubscribe(const char* topic);

    // Add more member functions as needed

private:
    HardwareSerial* serial;
    // Add any additional private members here
};

#endif // MQTTCLIENT_H