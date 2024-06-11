set(SRCROOT ${SRCROOT}/tests)
set(INCROOT ${INCROOT}/tests)

set(SRC
    ${SRCROOT}/binary_tests.cpp
)

set(INCDIRS ${SRCROOT}/../include)

set(TARGET_NAME ordurex_tests)
set(DISPLAY_NAME "OrdureX-Tests")
set(WORKING_DIRECTORY ${SRCROOT})

include(CompilerFlags)

add_executable(${TARGET_NAME} ${SRC})
target_include_directories(${TARGET_NAME} PRIVATE ${INCROOT})

source_group(${TARGET_NAME} FILES ${SRC})
set_target_properties(${TARGET_NAME} PROPERTIES FOLDER "tests")

target_link_libraries(${TARGET_NAME} PRIVATE binary gmock_main)

add_test(NAME "${DISPLAY_NAME}" COMMAND ${TARGET_NAME} WORKING_DIRECTORY ${WORKING_DIRECTORY})
