#include <gtest/gtest.h>

#include "../scheduler.h"

void testTask() {
  // Do nothing
}

void testSleepTask() {
  usleep(100000);
}

TEST(SchedulerTests, addTask) {
  task_t task = { 1000UL, 0UL, false, testTask };
  scheduler s;
  for (int i = 0; i < MAX_TASKS; i++)
  {
    EXPECT_TRUE(s.addTask(task));
  }
  
  EXPECT_FALSE(s.addTask(task));
}

TEST(SchedulerTests, run) {
  task_t task = { 0UL, 0UL, false, testSleepTask };
  scheduler s;
  s.addTask(task);
  s.run();
}