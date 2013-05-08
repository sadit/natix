//
//  Copyright 2013  Eric Sadit Tellez
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

namespace natix
{
	public class Fun
	{
		public Fun ()
		{
		}
		
		public static IEnumerable<Toutput> Map<Tinput,Toutput>(IEnumerable<Tinput> collection, Func<Tinput, Toutput> fun)
		{
			foreach (var item in collection) {
				yield return fun(item);
			}
		}

		public static Tinput Reduce<Tinput>(IEnumerable<Tinput> collection, Func<Tinput, Tinput, Tinput> fun)
		{
			Tinput prev = default(Tinput);
			int count = 0;
			foreach (var item in collection) {
				++count;
				if (count == 1) {
					prev = item;
				} else {
					prev = fun(prev, item);
				}
			}
			return prev;
		}
	}
}
