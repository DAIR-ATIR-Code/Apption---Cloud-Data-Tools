
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Accord.Statistics.Testing;
using RecognizerTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Xunit;

namespace DataToolsTest
{
    public class DistributionTest
    {
        [Fact]
        public void GivenDataset_ThenCheckNormal()
        {
            double[] samples = { 36, 415, 1431, 2651, 2772, 1936, 1112, 455, 133, 40, 14, 3, 2 };
            //double[] samples = { 19.73, 16.93, 16.78, 0.32, -7.44, -9.48, -5.75, -3.02, 35.34, 6.68, 24.33, 21.37, -6.86, -9.01, 1.72, -7.58, 44.8, 39.12, -26.28, -2.16 };

            var sw = new ShapiroWilkTest(samples);
            var w = sw.Statistic;
            var p = sw.PValue; 
            bool significant = sw.Significant;
            Assert.True(significant);
        }

        [Theory]
        [InlineData("Name", true)]
        [InlineData("NAME", true)]
        [InlineData("First Name", true)]
        [InlineData("Name with .", false)]
        [InlineData(@"Name with / ? \", false)]
        [InlineData(@"Name with 786", false)]
        [InlineData(@"Name with @", false)]
        [InlineData(@"", false)]
        public void GivenDecimalNumber_ThenParse(string data, bool expected)
        {
            Assert.Equal(IsLetterOnly(data), expected);
        }


        private bool IsLetterOnly(string data)
        {
            if (data.Length == 0)
                return false;
            for (var i = 0; i < data.Length; i++)
            {
                if (((data[i] < 'a' || data[i] > 'z') && (data[i] < 'A' || data[i] > 'Z')) && data[i] != ' ')
                    return false;
            }
            return true;
        }
    }
}



