set(SRCROOT ${PROJECT_SOURCE_DIR}/main)
set(INCROOT ${PROJECT_SOURCE_DIR}/main)

if(BUILD_TEST_SUITE)
    enable_testing()
    add_subdirectory(tests)
endif()

# Collect the main source files as a static library

set(SRC
    ${SRCROOT}/binary.cpp
    ${SRCROOT}/binary.h
    ${SRCROOT}/scheduler.cpp
    ${SRCROOT}/scheduler.h
)
set(INCDIRS ${INCROOT})

source_group("" FILES ${SRC})

set(TARGET_NAME binary)

include(CompilerFlags)

add_library(${TARGET_NAME} ${SRC})
target_include_directories(${TARGET_NAME} PRIVATE ${INCDIRS})
target_include_directories(${TARGET_NAME} PUBLIC ${INCROOT})

