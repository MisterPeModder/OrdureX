cmake_minimum_required(VERSION 3.16)

include(FetchContent)
    
# Since v1.14, GoogleTest now recommends building from the lastest commit
# FetchContent_Declare(
#     googletest
#     URL https://github.com/google/googletest/archive/a7f443b80b105f940225332ed3c31f2790092f47.zip
#     URL_HASH MD5=10f4c14c7a6af696e58dd635c452d7bc
# )
FetchContent_Declare(
    googletest
    GIT_REPOSITORY https://github.com/google/googletest.git
    GIT_TAG a7f443b80b105f940225332ed3c31f2790092f47
)

# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)

FetchContent_MakeAvailable(googletest)
