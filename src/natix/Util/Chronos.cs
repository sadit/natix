//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/Util/Chronos.cs
// 
using System;
using System.IO;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// Chronometer and counter, for testing and benchmarking
	/// </summary>
	public class Chronos
	{
		/// <summary>
		/// the last timestamp (in ticks)
		/// </summary>
		public long LastStart;
		/// <summary>
		/// The last end timestamp (in ticks)
		/// </summary>
		public long LastEnd;
		/// <summary>
		/// The call count.
		/// </summary>
		public long CallCount;
		/// <summary>
		/// The accumulated time (in ticks)
		/// </summary>
		public long AccTime;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.Chronos"/> class.
		/// </summary>
		public Chronos ()
		{
			this.LastStart = 0;
			this.LastEnd = 0;
			this.CallCount = 0;
			this.AccTime = 0;
		}
		
		/// <summary>
		/// Begin to count an event (synonym of Start)
		/// </summary>
		public void Begin ()
		{
			this.Start ();
		}
		
		/// <summary>
		/// Stop to count the event (synonym of End)
		/// </summary>
		public void Stop ()
		{
			this.End ();
		}
		
		/// <summary>
		/// Start to count an event (synonym of Begin)
		/// </summary>
		public void Start ()
		{
			this.LastStart = DateTime.Now.Ticks;
		}
		
		/// <summary>
		/// Ends to count an event (synonym of Stop)
		/// </summary>
		public void End ()
		{
			this.LastEnd = DateTime.Now.Ticks;
			this.CallCount++;
			var e = this.LastEnd - this.LastStart;
			this.AccTime += e;
			// this.PrintStats ();
		}
		
		/// <summary>
		/// Prints the current statistics
		/// </summary>
		public void PrintStats ()
		{
			this.PrintStats ("");
		}
		
		/// <summary>
		/// Prints the stats with a prefix
		/// </summary>
		public void PrintStats (string prefix)
		{
			this.PrintStats (prefix, Console.Out);
		}

		/// <summary>
		/// Prints the stats to Output (appending a prefix)
		/// </summary>
		public void PrintStats (string prefix, TextWriter Output)
		{
			foreach (var p in this.Statistics ()) {
				Output.WriteLine ("*** {0}{1}: {2}", prefix, p.Key, p.Value);
			}
		}
		
		/// <summary>
		/// Iterate over the statistics (key, value) pairs
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> Statistics ()
		{
			var e = this.LastEnd - this.LastStart;
			yield return new KeyValuePair<string, object> ("last-call", TimeSpan.FromTicks (e));
			yield return new KeyValuePair<string, object> ("num-call", this.CallCount);
			long callcount = this.CallCount;
			if (callcount == 0) {
				callcount++;
			}
			yield return new KeyValuePair<string, object> ("avg-time", TimeSpan.FromTicks (this.AccTime / callcount));
			yield return new KeyValuePair<string, object> ("avg-time-microsecs", this.AccTime / 10.0 / callcount);
			yield return new KeyValuePair<string, object> ("total-time", TimeSpan.FromTicks (this.AccTime));
		}
	}
}

