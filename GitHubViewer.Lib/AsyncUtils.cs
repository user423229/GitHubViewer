using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubViewer.Lib
{
    public static class AsyncUtils
    {
        /// <summary>
        /// For each element in a given input list, runs a task executing a given function. Tasks are run
        /// concurrently up to a given maximum concurrency value. When this value is exceeded, remaining tasks
        /// are run as soon as previous tasks are completed.
        /// </summary>
        /// <param name="inputList">List of input elements.</param>
        /// <param name="action">Function to run for each input element. The function receives an input element and
        /// returns any result (paired with the input element, so that a caller can match results with the input).</param>
        /// <param name="maxConcurrency">Maximum number of concurrent tasks to run.</param>
        /// <returns>
        /// Returns C# 8 async enumerable of task results, in task completion order. 
        /// </returns>
        public static async IAsyncEnumerable<(T1, T2)> RunForEach<T1, T2>(IReadOnlyList<T1> inputList, Func<T1, Task<(T1, T2)>> action, int maxConcurrency)
        {
            int inputListIndex = 0;
            List<Task<(T1, T2)>> runningTasks = new List<Task<(T1, T2)>>();
            
            for (int i = 0; i < maxConcurrency; i++)
            {
                if (inputListIndex >= inputList.Count)
                {
                    break;
                }
                runningTasks.Add(action(inputList[inputListIndex++]));
            }

            while (runningTasks.Any())
            {
                Task<(T1, T2)> finishedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(finishedTask);
                if (inputListIndex < inputList.Count)
                {
                    runningTasks.Add(action(inputList[inputListIndex++]));
                }
                yield return finishedTask.Result;
            }
        }
    }
}