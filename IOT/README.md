# OrdureX IOT

## Running the IOT device

> [!Note]
> You need to have Arduino CLI to compile for the Arduino Mega 2560.

```sh
# fill config file
cp main/config.h.sample main/config.h
# run with arduino cli
arduino-cli core update-index
# install Arduino Mega plateform
arduino-cli core install arduino:avr
# install some libs
arduino-cli lib install Keypad LiquidCrystal MFRC522 Servo
# compile project
arduino-cli compile --fqbn arduino:avr:mega main
# upload to Arduino
arduino-cli upload -p /dev/ttyACM0 --fqbn arduino:avr:mega main
```

## Testing

> [!Note]
> You need to have CMake 3.16+ and gcovr to run the tests.

### Using CMake

Running the tests:

```sh
# Configure the CMake project (run once)
cmake -B ./build -DCMAKE_BUILD_TYPE=Debug -DBUILD_TEST_SUITE=TRUE -DENABLE_TEST_COVERAGE=TRUE

# Build the project (run after moditying the source code)
cmake --build ./build --config Debug -j

# Run the tests
cd ./build && ctest -C Debug -j --output-on-failure
```

Displaying the test coverage as HTML:

```sh
gcovr . --exclude-unreachable-branches --exclude-throw-branches \
    --exclude main/tests/ --exclude build/_deps \
    --html --output ./coverage.html

# Open the coverage report in a browser
xdg-open ./coverage.html
```

```sh
gcovr --exclude main/tests/ --exclude build/_deps .
gcovr --exclude main/tests/ --exclude build/_deps --txt-metric branch .
```

### Using Makefile wrapper

A wrapper Makefile is provided to simplify the process of using CMake.

```sh
make test
```

```sh
# Plain text test coverage
make coverage 

# HTML test coverage
make coverage_html
xdg-open ./coverage.html
```

