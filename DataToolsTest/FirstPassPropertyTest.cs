
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DTHelperStd;
using RecognizerTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace DataToolsTest
{
    public class FirstPassPropertyTest
    {

        private readonly ITestOutputHelper output;

        public FirstPassPropertyTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("0.24", "0.35", "1", "2", "0", "1.22","True")]
        [InlineData("0.12", "0.124", "1.223", "45.23", "True")]
        [InlineData("1", "2", "3", "4", "5", "False")]
        [InlineData("1.11", "0.12", "abc", "1.22", "False")]
        [InlineData("0.11", "0.23", "2", "abc", "1.23", "False")]
        private void GivenNCOA_Then(params string[] dataArr)
        {
            var isInt = true;
            var isNumber = true;

            var letterCount = 0;

            for (int i = 0; i < dataArr.Length-1; i++)
            {
                if (isInt && !long.TryParse(dataArr[i], out var v))
                {
                    isInt = false;
                }


                if (isNumber && !double.TryParse(dataArr[i], out var e))
                {
                    isNumber = false;
                }

                if (StringHelper.IsLetterOnly(dataArr[i]))
                {
                    letterCount++;
                }
            }

            var isCompletelyLetter = (float)letterCount / (dataArr.Length-1) > 0.7;
            var isNumberWithLetter = !isCompletelyLetter && !isNumber;
            var isFloat = !isInt && isNumber;
            Assert.Equal(dataArr[dataArr.Length - 1], isFloat.ToString());
            output.WriteLine($"isInt: {isInt}");
            output.WriteLine($"isFloat: {isFloat}");
            output.WriteLine($"isNumber: {isNumber}");
            output.WriteLine($"isCompletelyLetter: {isCompletelyLetter}");
            output.WriteLine($"isNumberWithLetter: {isNumberWithLetter}");
        }



    }
}

