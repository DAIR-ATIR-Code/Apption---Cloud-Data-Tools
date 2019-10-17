
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using RecognizerTools.State;
using System.Threading;
using System.Threading.Tasks;

namespace RecognizerTools
{
    public interface IRecognizer
    {
        int Count { get; }
        bool IsMatch(string data, CancellationToken token);
        RecognizerSummary GetStatus();
        void IncrementCount();
        StorageType[] GetStorageTypes();
        RecognizerState CurrentState { get; }
        void StartBatch(RecognizerState state);
        RecognizerState EndBatch();
        string GetDescription();
    }
}

