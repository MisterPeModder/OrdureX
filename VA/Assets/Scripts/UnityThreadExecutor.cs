using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrdureX
{
    /// <summary>
    /// Provides utility methods to execute code on the Unity main thread.
    /// </summary>
    /// https://stackoverflow.com/questions/41330771/use-unity-api-from-another-thread-or-call-a-function-in-the-main-thread
    public class UnityThreadExecutor : MonoBehaviour
    {
        private static UnityThreadExecutor instance;

        private readonly static List<Action> actionQueue = new();
        private readonly List<Action> currentActions = new();
        private volatile static bool noActionsQueued = false;

        /// <summary>
        /// Initializes the UnityThreadExecutor (if not already initialized).
        /// </summary>
        public static void Init(bool visible = false)
        {
            if (instance != null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var obj = new GameObject("UnityThreadExecutor");
                if (!visible)
                {
                    obj.hideFlags = HideFlags.HideAndDontSave;
                }
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<UnityThreadExecutor>();
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Schedules the given action to execute in the next call to Update().
        /// </summary>
        public static void Execute(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            lock (actionQueue)
            {
                actionQueue.Add(action);
                noActionsQueued = false;
            }
        }

        public void Update()
        {
            if (noActionsQueued)
            {
                return;
            }

            // Clear old actions and swap queues
            currentActions.Clear();
            lock (actionQueue)
            {
                currentActions.AddRange(actionQueue);
                actionQueue.Clear();
                noActionsQueued = true;
            }

            for (int i = 0; i < currentActions.Count; i++)
            {
                currentActions[i].Invoke();
            }
        }

        public void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
