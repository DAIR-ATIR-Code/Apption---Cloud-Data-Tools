
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Concurrent;
using System.Threading;
using DataTools;
using DTHelperStd;
using System.Linq;
using System.Collections.Generic;
using RecognizerTools.State;

namespace RecognizerTools
{
    public abstract class Recognizer : IRecognizer
    {
        protected int _count = 0;
        public int Count { get { return _count; } }
        public RecognizerState CurrentState { get; private set; }
        private bool _initialized = false;

        protected int IncrementStats(int length)
        {
            if (!_initialized) throw new InvalidOperationException("Recognizer not initialized");
            return Interlocked.Increment(ref CurrentState.Statistics[Math.Min(RecognizerState.MAX_STAT_LENGTH, length)]);
        }

        public List<(float, float)> GetSummaryStatistics()
        {
            return CurrentState.Statistics.Select((count, index) => ((float)index, (float)count)).Where(tp => tp.Item2 > 0).ToList();
        }

        public virtual void StartBatch(RecognizerState state)
        {
            _initialized = true;
            CurrentState = state;            
            _count = state.Count;
        }

        public virtual string GetDescription()
        {
            return this.GetType().ToString();
        }

        public virtual RecognizerState EndBatch()
        {
            _initialized = false;
            CurrentState.Count = _count;
            return CurrentState;
        }

        public abstract bool IsMatch(string data, CancellationToken cancellationToken);

        public abstract RecognizerSummary GetStatus();

        public void IncrementCount()
        {
            Interlocked.Increment(ref _count);
        }

        public virtual StorageType[] GetStorageTypes()
        {
            var classAttribute = Attribute.GetCustomAttribute(this.GetType(), typeof(StorageTypesAttribute)) as StorageTypesAttribute;
            if (classAttribute is null)
                throw new InvalidOperationException($"Class {this.GetType().ToString()} does not have a StorageType attribute");
            return classAttribute.StorageTypes;
            

        }
    }
}

