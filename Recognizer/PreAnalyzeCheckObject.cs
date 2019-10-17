
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RecognizerTools
{
    public class PreAnalyzeCheckObject
    {
        private const float HEADER_OFFSET = 0.8f;
        private const int READLINE_NUMBER = 50;
        private readonly string filePath;

        public bool HasHeaders { get; set; } = true;
        public SeparatorType SeparatorType { get; set; } = SeparatorType.Comma;
        public float HeadersProbability { get; set; }

        public PreAnalyzeCheckObject(string filePath)
        {
            this.filePath = filePath;
        }

        public void PreAnalyze()
        {
            SeparatorType = DetermineSeparator();
            //HasHeaders = HasHeader();
        }

        private char GetSeparatorBySeparatorType(SeparatorType separatorType)
        {
            switch (separatorType)
            {
                case SeparatorType.Comma:
                    return ',';
                case SeparatorType.Pipe:
                    return '|';
                case SeparatorType.Tab:
                    return '\t';
                case SeparatorType.Semicolon:
                    return ';';
                default:
                    return ',';
            }
        }

        public SeparatorType DetermineSeparator()
        {
            var numberOfFieldsDic = new Dictionary<SeparatorType, int>()
            {
                { SeparatorType.Comma, 0 },
                { SeparatorType.Pipe, 0 },
                { SeparatorType.Tab, 0 },
                { SeparatorType.Semicolon, 0 },
            };
            var IsFound = false;
            using (var source = new StreamReader(filePath))
            {
                var lineNumber = 0;
                string newLine;
                var keys = new List<SeparatorType>(numberOfFieldsDic.Keys);
                while((newLine = source.ReadLine()) != null && lineNumber < READLINE_NUMBER)
                {
                    foreach(var key in keys)
                    {
                        var numberOfField = newLine.Split(GetSeparatorBySeparatorType(key)).Length;
                        if (numberOfField > numberOfFieldsDic[key])
                        {
                            numberOfFieldsDic[key] = numberOfField;
                            IsFound = true;
                        } 
                    }
                    lineNumber++;
                }

            }
            if (!IsFound)
            {
                return SeparatorType.Custom;
            }
            return numberOfFieldsDic.Aggregate((s, n) => s.Value > n.Value ? s : n).Key;
        }


        //public bool HasHeader()
        //{
        //    var source = new StreamReader(filePath);
        //    var line = source.ReadLine();
        //    var separator = GetSeparatorBySeparatorType(SeparatorType);
        //    string[] values = line.Split(separator);

        //    //step 1: Check for existence of number value inside the first row
        //    foreach (var value in values)
        //    {
        //        if (double.TryParse(value, out var d))
        //            return false;
        //    }

        //    //step 2: Check for duplicate values inside the first row
        //    if (values.GroupBy(n => n).Any(c => c.Count() > 1))
        //    {
        //        return false;
        //    }

        //    //step 3: Check by some string recognizer
        //    //Initialize the reference data dictionary
        //    if (ReferenceHelper.ReferenceDic.Count == 0)
        //    {
        //        ReferenceHelper.InitializeReferenceDict();
        //    }
        //    var score = 0;
        //    // we filter the defined classes according to the interfaces they implement
        //    var recognizerToolsAssembly = typeof(Recognizer).GetTypeInfo().Assembly;

        //    var stringRecognzierTypes = recognizerToolsAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ILetterRecognizer)) && type != typeof(LastNameRecognizer) && type != typeof(NameRecognizer) && type != typeof(LetterRecognizer)).ToList();

        //    var textRecognzierTypers = recognizerToolsAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ILetterWithNumberRecognizer)) && type != typeof(LastNameRecognizer) && type != typeof(UsernameRecognizer) && type != typeof(LetterWithNumberRecognizer)).ToList();

        //    var recognizerList = new List<Recognizer>();
        //    foreach (var type in stringRecognzierTypes)
        //    {
        //        var instance = (Recognizer)Activator.CreateInstance(type, new Object[] { null });
        //        recognizerList.Add(instance);
        //    }
        //    foreach (var type in textRecognzierTypers)
        //    {
        //        var instance = (Recognizer)Activator.CreateInstance(type, new Object[] { null });
        //        recognizerList.Add(instance);
        //    }

        //    foreach (var value in values)
        //    {
        //        foreach (var recognizer in recognizerList)
        //        {
        //            if (recognizer.IsMatch(value, CancellationToken.None))
        //            {
        //                score++;
        //                break;
        //            }
        //        }
        //    }
        //    HeadersProbability = 1 - ((float)score / values.Length);
        //    return HeadersProbability > HEADER_OFFSET;
        //}
    }
}

