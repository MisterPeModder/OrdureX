cmake_minimum_required(VERSION 3.16)

project(OrdureX LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED TRUE)

option(BUILD_TEST_SUITE "Build the test suite" OFF)
option(ENABLE_COVERAGE "Enable coverage reporting" OFF)

# Add ./cmake to the module path
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

include(DetectCompiler)

add_subdirectory(main)

if(BUILD_TEST_SUITE)
    find_package(googletest REQUIRED)
    enable_testing()
endif()
