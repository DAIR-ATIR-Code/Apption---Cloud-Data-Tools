
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataTools;
using DTHelperStd;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class StockSymbolsRecognizer : Recognizer, ILetterRecognizer, IMediumStringRecognizer
    {
        private ColumnMetadata metadata;
       
        private const int MAX_LENGTH = 10;
        public StockSymbolsRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);

            if (!cancellationToken.IsCancellationRequested)
            {
                if (ReferenceHelper.ReferenceDic["NASDAQList"].Contains(data) || ReferenceHelper.ReferenceDic["NYSEList"].Contains(data))
                    return true;
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = MAX_LENGTH, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Stock Symbols";
        }
    }
}

