//
//  Copyright 2014  sadit
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace natix
{
	/// <summary>
	/// Parallel task like methods to run expensive procedures
	/// </summary>
	/// <remarks>
	/// The execution process will try to use real threads whenever is possibles.
	/// This can be relatively expensive on light weight procedures.
	/// Use TPL methods if you are in doubt.
	/// <remarks/>
	public class LongParallel
	{
		public LongParallel ()
		{
		}

		static int SLEEP_MILLISECONDS = 100;
		static int DEFAULT_THREADS = System.Environment.ProcessorCount;
		static List<Task> INVOKE_QUEUE = new List<Task>(System.Environment.ProcessorCount);

		public static Action CreateClosureFor(int i, Action<int> fun)
		{
			return () => fun (i);
		}

		public static Action CreateClosureForEach<T>(T i, Action<T> fun)
		{
			return () => fun (i);
		}

		public static void Invoke(Action action)
		{
			CleanQueue (INVOKE_QUEUE);
			var task = new Task (action, TaskCreationOptions.LongRunning);
			INVOKE_QUEUE.Add (task);
			task.Start ();
		}

		public static void For(int fromInclusive, int toExclusive, Action<int> fun, int max_threads = -1)
		{
			if (max_threads == -1) {
				max_threads = DEFAULT_THREADS;
			}

			var queue = new List<Task> (max_threads);
			while (fromInclusive < toExclusive) {
				var task = new Task (CreateClosureFor(fromInclusive, fun), TaskCreationOptions.LongRunning);
				queue.Add (task);
				task.Start ();
				++fromInclusive;
				if (queue.Count >= max_threads) {
					WaitOne (queue, max_threads);
				}
			}
			WaitAll (queue);
		}

		public static void ForEach<T>(ICollection<T> col, Action<T> fun, int max_threads = -1)
		{
			if (max_threads == -1) {
				max_threads = DEFAULT_THREADS;
			}

			var queue = new List<Task> (max_threads);
			foreach (var item in col) {
				var task = new Task (CreateClosureForEach<T>(item, fun), TaskCreationOptions.LongRunning);
				queue.Add (task);
				task.Start ();
				if (queue.Count >= max_threads) {
					WaitOne (queue, max_threads);
				}
			}
			WaitAll (queue);
		}

		public static void WaitOne(List<Task> queue, int max_threads)
		{
			while (queue.Count >= max_threads) {
				if (CleanQueue(queue) == 0) {
					System.Threading.Thread.Sleep (SLEEP_MILLISECONDS);
				}
			}
		}

		public static void WaitAll(List<Task> queue)
		{
			while (queue.Count > 0) {
				if (CleanQueue(queue) == 0) {
					System.Threading.Thread.Sleep (SLEEP_MILLISECONDS);
				}
			}
		}

		public static void InvokeWaitAll()
		{
			while (INVOKE_QUEUE.Count > 0) {
				if (CleanQueue(INVOKE_QUEUE) == 0) {
					System.Threading.Thread.Sleep (SLEEP_MILLISECONDS);
				}
			}
		}

		static int CleanQueue(List<Task> queue)
		{
			int removed = 0;
			for (int i = 0; i < queue.Count; ++i) {
				var status = queue [i].Status;
				if (status == TaskStatus.RanToCompletion || status == TaskStatus.Canceled || status == TaskStatus.Faulted) {
					var last = queue.Count - 1;
					// queue [i].Dispose ();
					queue [i] = queue [last];
					queue.RemoveAt (last);
					++removed;
				}
			}
			return removed;
		}
	}
}

