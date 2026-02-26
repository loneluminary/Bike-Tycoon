using System;
using System.Collections;
using System.Threading.Tasks;

namespace Utilities.Extensions
{
    public static class TaskExtensions
    {
        /// Wraps the provided object into a completed Task.
        /// <param name="obj">The object to be wrapped in a Task.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>A completed Task containing the object.</returns>
        public static Task<T> AsCompletedTask<T>(this T obj) => Task.FromResult(obj);

        /// Converts the Task into an IEnumerator for Unity coroutine usage.
        /// <param name="taskData">The Task to convert.</param>
        /// <returns>An IEnumerator representation of the Task.</returns>
        public static IEnumerator AsCoroutine(this Task taskData)
        {
            while (!taskData.IsCompleted) yield return null;
            // When used on a faulted Task, GetResult() will propagate the original exception. 
            // see: https://devblogs.microsoft.com/pfxteam/task-exception-handling-in-net-4-5/
            taskData.GetAwaiter().GetResult();
        }

        /// Marks a task to be forgotten, meaning any exceptions thrown by the task will be caught and handled.
        /// <param name="taskData">The task to be forgotten.</param>
        /// <param name="onException">The optional action to execute when an exception is caught. If provided, the exception will not be rethrown.</param>
        public static async void Forget(this Task taskData, Action<Exception> onException = null)
        {
            try
            {
                await taskData;
            }
            catch (Exception exception)
            {
                if (onException == null) throw exception;

                onException(exception);
            }
        }

        /// Repeatedly polls a condition until it returns true or a timeout occurs.
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="timeoutMs">Maximum time to wait in milliseconds. Use -1 for no timeout.</param>
        /// <param name="pollIntervalMs">Time between condition checks in milliseconds. Default is 33ms (≈ one frame at 30 FPS).</param>
        /// <returns>True if the condition was met; false if the timeout was reached.</returns>
        /// <exception cref="ArgumentNullException">Thrown when condition is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pollIntervalMs is not positive.</exception>
        public static async Task<bool> WaitUntil(this Func<bool> condition, int timeoutMs = -1, int pollIntervalMs = 33)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (pollIntervalMs <= 0) throw new ArgumentOutOfRangeException(nameof(pollIntervalMs), "Poll interval must be positive");

            var cancelled = false;
            var waitTask = RunWaitLoop(condition, pollIntervalMs, () => cancelled);

            if (timeoutMs < 0)
            {
                await waitTask;
                return true;
            }

            var timeoutTask = Task.Delay(timeoutMs);
            var finished = await Task.WhenAny(waitTask, timeoutTask);

            if (finished != waitTask)
            {
                cancelled = true;
                return false;
            }

            return true;
        }

        static async Task RunWaitLoop(Func<bool> condition, int pollIntervalMs, Func<bool> cancelled)
        {
            while (!condition() && !cancelled())
            {
                await Task.Delay(pollIntervalMs);
            }
        }
    }
}