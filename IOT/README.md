# OrdureX IOT

## Running the IOT device

```sh
# fill config file
cp main/config.h.sample main/config.h
# run with arduino cli
arduino-cli core update-index
# install Arduino Mega plateform
arduino-cli core install arduino:avr
# compile project
arduino-cli compile --fqbn arduino:avr:mega main
# upload to Arduino
arduino-cli upload -p /dev/ttyACM0 --fqbn arduino:avr:mega main
```
