#include "scheduler.h"

#ifdef ARDUINO
#include "Arduino.h"
#define GET_TIME millis()
#else
#include <chrono>
#define GET_TIME (unsigned long)std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count()
#endif

Scheduler::Scheduler() {
  for (int i = 0; i < MAX_TASKS; i++) {
    tasks[i].delay = 0;
    tasks[i].lastRun = 0;
    tasks[i].loop = false;
    tasks[i].task = nullptr;
  }
}

Scheduler::~Scheduler() {
}

bool Scheduler::addTask(task_t task) {
  for (int i = 0; i < MAX_TASKS; i++) {
    if (tasks[i].task == nullptr) {
      tasks[i] = task;
      return true;
    }
  }
  return false;
}

void Scheduler::run() {
  for (int i = 0; i < MAX_TASKS; i++) {
    if (tasks[i].task != nullptr && GET_TIME >= tasks[i].lastRun + tasks[i].delay) {
      tasks[i].task(tasks[i].context);
      if (tasks[i].loop) {
        tasks[i].lastRun = GET_TIME;
      } else {
        tasks[i].task = nullptr;
      }
    }
  }
}