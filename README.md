# OrdureX

*"Mixing IoT and AR since 2024!"*

[Link to original repository](https://github.com/MisterPeModder/OrdureX)

# Start MQTT broker

```sh
docker compose up -d
```

Manage users:

```sh
# Copy password file
cp mosquitto/mqtt_passwd.example mosquitto/mqtt_passwd
sudo chown root:root mosquitto/mqtt_passwd
sudo chmod 700 mosquitto/mqtt_passwd

# Alternative way
touch mosquitto/mqtt_passwd
sudo chown root:root mosquitto/mqtt_passwd
sudo chmod 700 mosquitto/mqtt_passwd
# Create a user "arduino" with password "password"
docker compose exec mosquitto mosquitto_passwd -b /mosquitto/config/mqtt_passwd arduino password
```

Publish something from the broker:

```sh
# Publish "coucou" payload on "hey" topic
docker compose exec mosquitto mosquitto_pub -u arduino -P password -m coucou -t hey
```
