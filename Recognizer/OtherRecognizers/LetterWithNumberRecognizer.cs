
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataTools;
using DTHelperStd;

namespace RecognizerTools
{
    [StorageTypes(new[]{ StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar, StorageType.Datetime,
            StorageType.Datetime2, StorageType.Bigint, StorageType.Bit,StorageType.Decimal,StorageType.Int, StorageType.Smallint })]
    public class LetterWithNumberRecognizer : Recognizer, ILetterWithNumberRecognizer, IShortStringRecognizer, IMediumStringRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;

        public LetterWithNumberRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                return RegexHelper.EVERYTHING_PATTERN.IsMatch(data);
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength + 1, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Text (with number)";
        }


    }
}

