//
//  Copyright 2013  Eric Sadit Tellez Avila
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
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
    public class DistanceHistogram
    {
        public Dictionary<double,int> H;
        public double bin_size;
        public int count;

        public DistanceHistogram (double bin_size)
        {
            this.H = new Dictionary<double, int>();
            this.bin_size = bin_size;
        }

        public void Add (double d)
        {
            var key = ((int)(d / this.bin_size)) * this.bin_size;
            int value;
            if (!this.H.TryGetValue(key, out value)) {
                value = 0;
            }
            this.H[key] = value + 1;
            ++this.count;
        }

        public List<double> SortedKeys()
        {
            var L = new List<double>(this.H.Keys);
            L.Sort();
            return L;
        }

        public string AsPylab (string varname = "histogram")
        {
            var s = new StringBuilder();
            s.AppendLine("from pylab import plot, Line2D, show");
            s.AppendFormat("{0}_x = [", varname);
            var L = this.SortedKeys();
            for (int i = 0; i < L.Count; ++i) {
                s.AppendFormat("{0}, ", L[i]);
            }
            s.AppendLine("]");
            s.AppendFormat("{0}_y = [", varname);
            for (int i = 0; i < L.Count; ++i) {
                s.AppendFormat("{0}, ", this.H[L[i]]);
            }
            s.AppendLine("]");
            s.AppendLine(String.Format("plot({0}_x, {0}_y); show(); ", varname));
            return s.ToString();
        }

        public string AsSage (string varname = "histogram")
        {
            var s = new StringBuilder();
            s.AppendFormat("{0} = [", varname);
            var L = this.SortedKeys();
            for (int i = 0; i < L.Count; ++i) {
                s.AppendFormat("({0},{1}), ", L[i], this.H[L[i]]);
            }
            s.AppendLine("]");
            s.AppendLine(String.Format("P = plot([]); P += line({0}); P.show(); ", varname));
            return s.ToString();
        }

    }
}

