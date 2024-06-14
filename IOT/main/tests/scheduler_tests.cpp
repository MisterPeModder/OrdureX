#include <gtest/gtest.h>
#include <iostream>

#include "../scheduler.h"

#define ARGUMENT_TASK 124

struct SomeContext {
  int a;
};

void testTask(void* context) {
  // This is garbage code, but it's just for testing purposes
  std::cout << context << std::endl;
}

void testSleepTask(void* context) {
  std::cout << context << std::endl;
  usleep(100000);
}

void testArgumentTask(void* context) {
  SomeContext* c = static_cast<SomeContext*>(context);
  ASSERT_EQ(c->a, ARGUMENT_TASK);
}

TEST(SchedulerTests, addTask) {
  task_t task = { 1000UL, 0UL, false, testTask, nullptr };
  scheduler s;
  for (int i = 0; i < MAX_TASKS; i++)
  {
    EXPECT_TRUE(s.addTask(task));
  }
  
  EXPECT_FALSE(s.addTask(task));
}

TEST(SchedulerTests, run) {
  task_t task = { 0UL, 0UL, false, testSleepTask, nullptr };
  scheduler s;
  s.addTask(task);
  s.run();
}

TEST(SchedulerTests, runWithArgument) {
  SomeContext c = { ARGUMENT_TASK };
  task_t task = { 0UL, 0UL, false, testArgumentTask, &c };
  scheduler s;
  s.addTask(task);
  s.run();
}