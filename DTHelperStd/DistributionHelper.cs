
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Testing;
using Accord.Statistics;

namespace DTHelperStd
{
    public static class DistributionHelper
    {
        private const int SAMPLE_SIZE = 40;

        public static Dictionary<string, string> NormalTest(List<(float x, float y)> dataPoints, double minX, double maxX)
        {
            var totalCount = dataPoints.Select(p => p.y).Sum();
            var sampleSize = new List<int>();
            var samplesList = new List<double>();
            var bucketRange = Math.Ceiling((maxX - minX) / dataPoints.Count);
            for (var j= 0; j < dataPoints.Count; j++)
            {
                var r = new Random();
                for (var i=0; i< Math.Floor((dataPoints[j].y / totalCount) *SAMPLE_SIZE); i++)
                {
                    samplesList.Add(r.NextDouble() * ((dataPoints[j].x -bucketRange/2) - (dataPoints[j].x + bucketRange/2)) + (dataPoints[j].x - bucketRange/2));
                }
                
            }
            var result = new Dictionary<string, string>();
            var samplesArr = samplesList.ToArray();
            var sw = new ShapiroWilkTest(samplesArr);
            Measures.Mean(samplesArr);
            result.Add("Statistic: ", string.Format("{0:n2}",sw.Statistic));
            result.Add("- PValue: ", string.Format("{0:n2}", sw.PValue));
            result.Add("- Distribution: ", $"{(!sw.Significant ==true ?"Normal Distribution" : "Unknown Distribution")}");
            result.Add("- Mean: ", Measures.Mean(samplesArr).ToString("n2"));
            result.Add("- Variance: ", Measures.Variance(samplesArr).ToString("n2"));
            result.Add("- Standard Deviation: ", Measures.StandardDeviation(samplesArr).ToString("n2"));
            return result;
        }
    }
}

