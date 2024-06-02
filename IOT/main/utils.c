#include <stdlib.h>
#include <string.h>

/**
 * Concat strings. Do not forget to free returned value!
 */
const char* concat2(const char* str1, const char* str2) {
  char* str = (char*)malloc(sizeof(char) * (strlen(str1) + strlen(str2) + 1));
  strcpy(str, str1);
  strcat(str, str2);
  return str;
}

/**
 * Concat strings. Do not forget to free returned value!
 */
const char* concat4(const char* str1, const char* str2, const char* str3, const char* str4) {
  char* str = (char*)malloc(sizeof(char) * (strlen(str1) + strlen(str2) + strlen(str3) + strlen(str4) + 1));
  strcpy(str, str1);
  strcat(str, str2);
  strcat(str, str3);
  strcat(str, str4);
  return str;
}

/**
 * Concat strings. Do not forget to free returned value!
 */
const char* concat5(const char* str1, const char* str2, const char* str3, const char* str4, const char* str5) {
  char* str = (char*)malloc(sizeof(char) * (strlen(str1) + strlen(str2) + strlen(str3) + strlen(str4) + strlen(str5) + 1));
  strcpy(str, str1);
  strcat(str, str2);
  strcat(str, str3);
  strcat(str, str4);
  strcat(str, str5);
  return str;
}