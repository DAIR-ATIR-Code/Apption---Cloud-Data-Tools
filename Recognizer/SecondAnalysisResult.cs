
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace RecognizerTools
{
    public class SecondAnalysisResult : IObservable<SecondAnalysisResult>
    {
        private static readonly float MIN_PROB = 0.4f;

        public ConcurrentDictionary<Type, RecognizerSummary> SummaryDataByRecognizer { get; set; } = null;
        public ConcurrentDictionary<Type, float> ProbabilityByRecognizer { get; set; } = null;
        public ConcurrentDictionary<Type, float> ProbabilityWithoutNullByRecognizer { get; set; } = null;
        //public Dictionary<DataType, Type> DataTypeToRecognizerTypeMap { get; } = new Dictionary<DataType, Type>();
        public int ColumnIndex { get; set; } = 0;

        public SecondAnalysisResult(int index)
        {
            SummaryDataByRecognizer = new ConcurrentDictionary<Type, RecognizerSummary>();
            ProbabilityByRecognizer = new ConcurrentDictionary<Type, float>();
            ProbabilityWithoutNullByRecognizer = new ConcurrentDictionary<Type, float>();
            ColumnIndex = index;
        }
        public void CleanupProb()
        {
            foreach (var kvp in ProbabilityWithoutNullByRecognizer)
            {
                if (kvp.Value < MIN_PROB)
                {
                    ProbabilityByRecognizer?.TryRemove(kvp.Key, out float value);
                    ProbabilityWithoutNullByRecognizer?.TryRemove(kvp.Key, out float value1);
                }
                if (kvp.Value > MIN_PROB && !IsBasicRecognizer(kvp.Key))
                {
                    ProbabilityByRecognizer.TryRemove(typeof(NumberRecognizer), out var value);
                    ProbabilityByRecognizer.TryRemove(typeof(LetterRecognizer), out value);
                    ProbabilityByRecognizer.TryRemove(typeof(LetterWithNumberRecognizer), out value);
                    ProbabilityWithoutNullByRecognizer.TryRemove(typeof(NumberRecognizer), out value);
                    ProbabilityWithoutNullByRecognizer.TryRemove(typeof(LetterRecognizer), out value);
                    ProbabilityWithoutNullByRecognizer.TryRemove(typeof(LetterWithNumberRecognizer), out value);
                }
            }       
        }

        private bool IsBasicRecognizer(Type recognizer)
        {
            return recognizer == typeof(NumberRecognizer) || recognizer == typeof(LetterRecognizer) || recognizer == typeof(LetterWithNumberRecognizer);
        }

        public IDisposable Subscribe(IObserver<SecondAnalysisResult> observer)
        {
            observer.OnNext(this);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}

