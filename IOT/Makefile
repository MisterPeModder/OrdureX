# Wrapper Makefile that delegates to CMake
# Created for ease of use, because CMake is a pain to use

TARGET := build/main/libbinary.a
BUILD_DIR := build

CONFIG_FLAGS := -G"Unix Makefiles" -B$(BUILD_DIR)

CONFIG_FLAGS_RELEASE := -DCMAKE_BUILD_TYPE=Release -DBUILD_TEST_SUITE=TRUE \
$(CONFIG_FLAGS)
CONFIG_FLAGS_DEBUG := -DCMAKE_BUILD_TYPE=Debug -DBUILD_TEST_SUITE=TRUE \
-DENABLE_TEST_COVERAGE=TRUE $(CONFIG_FLAGS)

BUILD_FLAGS_RELEASE := --config Release
BUILD_FLAGS_DEBUG := --config Debug

TEST_FLAGS := -T test --output-on-failure --output-log test.log

# Build Targets
all: $(TARGET)

$(TARGET):
	cmake $(CONFIG_FLAGS_RELEASE)
	cmake --build $(BUILD_DIR) $(BUILD_FLAGS_RELEASE)

# Remove Target's Build Files
clean:
	rm -rf $(BUILD_DIR)/CMakeFiles
	rm -rf $(BUILD_DIR)/src/CMakeFiles
	rm -rf ./coverage.html
	rm -rf *.gcno
	rm -rf *.gcda

# Remove Targets
fclean:
	rm -rf $(BUILD_DIR)
	rm -f $(TARGET)

# Recompile all
re: fclean all

# Build Targets in debug mode
debug:
	cmake $(CONFIG_FLAGS_DEBUG)
	cmake --build $(BUILD_DIR) $(BUILD_FLAGS_DEBUG)

# Run Unit Tests
test: fclean debug
	(cd $(BUILD_DIR) && ctest $(TEST_FLAGS)) || true

# Display Code Coverage
coverage:
	gcovr --exclude main/tests/ --exclude build/_deps .
	gcovr --exclude main/tests/ --exclude build/_deps --txt-metric branch .

coverage_html:
	gcovr . --exclude-unreachable-branches --exclude-throw-branches \
		--exclude main/tests/ --exclude build/_deps \
		--html --output ./coverage.html

.PHONY: \
	all \
	clean fclean re \
	debug \
	run_tests run_test tests_run test_run coverage
