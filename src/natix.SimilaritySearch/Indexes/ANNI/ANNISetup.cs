//
//  Copyright 2015  Eric S. Tellez <eric.tellez@infotec.com.mx>
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

namespace natix.SimilaritySearch
{
	public class ANNISetup
	{
		public int ExpectedK = 1;
		public double AlphaStop = 0.001;
		public int StepWidth = 128;
		public int NumberQueries = 64;
		public PivotSelector Selector = null;

		public ANNISetup (int n, int expectedK)
		{
			this.Selector = new PivotSelectorRandom (n);
			this.StepWidth = (int)Math.Sqrt (n) + 1;
			this.NumberQueries = 32; // (int)Math.Log (n, 2) + 1;
			this.ExpectedK = expectedK;
		}

		public ANNISetup (PivotSelector sel, int expectedK, double alphaStop, int step, int numQueries)
		{
			this.Selector = sel;
			this.ExpectedK = expectedK;
			this.AlphaStop = alphaStop;
			this.StepWidth = step;
			this.NumberQueries = numQueries;
		}

		public override string ToString()
		{
			return String.Format ("[ExpectedK: {0}, AlphaStop: {1}, StepWidth: {2}, NumberQueries: {3}]",
				this.ExpectedK, this.AlphaStop, this.StepWidth, this.NumberQueries);
		}
	}
}

