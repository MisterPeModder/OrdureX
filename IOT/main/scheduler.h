#ifndef SCHEDULER_H
#define SCHEDULER_H

#define MAX_TASKS 20

struct task_t {
  // Delay before the task is executed (real execution delay may vary)
  unsigned long delay;
  // Time when the task was last executed
  unsigned long lastRun;
  // Function must be called indefinitely
  bool loop;
  // Function to be executed
  void (*task)(void* context);
  // Context for the task
  void* context;
};

class scheduler {
public:
  scheduler();
  ~scheduler();

  bool addTask(task_t task);
  void run();
private:
  task_t tasks[MAX_TASKS];
};

#endif  // SCHEDULER_H