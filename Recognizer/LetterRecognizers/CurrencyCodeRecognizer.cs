
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
    [StorageTypes(new[] { StorageType.Char, StorageType.Varchar, StorageType.Nchar, StorageType.NVarchar })]
    public class CurrencyCodeRecognizer : Recognizer, ILetterRecognizer, IShortStringRecognizer
    {
        private ColumnMetadata metadata;

        private const int MAX_LENGTH = 5;

        public CurrencyCodeRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);

            if (!cancellationToken.IsCancellationRequested)
            {
                if (ReferenceHelper.ReferenceDic["CurrencyCode"].Contains(data))
                    return true;               
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Currency Code";
        }
    }
}

