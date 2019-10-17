
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using RecognizerTools;
using System.Reflection;
using System.Threading;
using DTHelperStd;
using DataTools;

namespace DataToolsTest
{
    public class HeadersDetectedTest : DataToolsTest
    {
        private readonly ITestOutputHelper output;

        public HeadersDetectedTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact (Skip ="Has error in initializing Reference dictionary")]
        //[InlineData(@"..\..\..\..\Samples\dummy_data.csv")]
        //[InlineData(@"..\..\..\..\Samples\General.csv")]
        //[InlineData(@"..\..\..\..\Samples\NCOA_EXTRACT_20181120.txt")]
        //[InlineData(@"..\..\..\..\Samples\DemoFile.txt")]
        //[InlineData(@"..\..\..\..\Samples\fakeData.csv")]
        private void GivenFile_ThenDetectHasHeaders()
        {
            ReferenceHelper.InitializeReferenceDict();
            var file = new StreamReader(@"..\..\..\..\Samples\money.csv");
            var header = file.ReadLine();
            var fileSeperator = ',';
            output.WriteLine(header);
            string[] values = header.Split(fileSeperator);
            var score = ComputeHeaderProbability(values);
            output.WriteLine($"Probability: {score}");
            Assert.True(score > 0.8, $"{score}");
        }

        [Theory]
        [InlineData(@"..\..\..\..\Samples\dummy_data.csv",SeparatorType.Comma)]
        [InlineData(@"..\..\..\..\Samples\General.csv", SeparatorType.Comma)]
        [InlineData(@"..\..\..\..\Samples\NCOA_EXTRACT_20181120.txt", SeparatorType.Pipe)]
        [InlineData(@"..\..\..\..\Samples\DemoFile.txt", SeparatorType.Comma)]
        [InlineData(@"..\..\..\..\Samples\fakeData.csv", SeparatorType.Comma)]
        public void GivenFile_ThenDetectHeaders(string fileName,SeparatorType expected)
        {
            var preCheckObj = new PreAnalyzeCheckObject(fileName);
            preCheckObj.PreAnalyze();
            Assert.Equal(expected, preCheckObj.SeparatorType);
        }

        private float ComputeHeaderProbability(string[] values)
        {
            //step 1: Check for existence of number value inside the first row
            foreach(var value in values)
            {
                if (double.TryParse(value, out var d))
                    return 0;
            }

            //step 2: Check for duplicate values inside the first row
            if (values.GroupBy(n => n).Any(c => c.Count() > 1))
            {
                return 0;
            }

            //step 3: check by some string recognizer
            var score = 0;
            var currentAssembly = typeof(Recognizer).GetTypeInfo().Assembly;

            // we filter the defined classes according to the interfaces they implement
            var stringRecognzierTypes = currentAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ILetterRecognizer)) && type != typeof(LastNameRecognizer) && type != typeof(NameRecognizer) && type != typeof(LetterRecognizer)).ToList();

            var textRecognzierTypers = currentAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ILetterWithNumberRecognizer)) && type != typeof(LastNameRecognizer) && type != typeof(UsernameRecognizer) && type != typeof(LetterWithNumberRecognizer)).ToList();
            
            var recognizerList = new List<Recognizer>();
            foreach(var type in stringRecognzierTypes)
            {
                var instance = (Recognizer)Activator.CreateInstance(type, new Object[] { null});
                recognizerList.Add(instance);
            }
            foreach(var type in textRecognzierTypers)
            {
                var instance = (Recognizer)Activator.CreateInstance(type, new Object[] { null });
                recognizerList.Add(instance);
            }

            foreach(var value in values)
            {
                foreach(var recognizer in recognizerList)
                {
                    if (recognizer.IsMatch(value, CancellationToken.None))
                    {
                        score++;
                        break;
                    }
                }
            }

            return 1- ((float) score/ values.Length);
        }
    }
}

