
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class IPv4Recognizer : Recognizer, INumberRecognizer, IMediumStringRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;
        public IPv4Recognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                return RegexHelper.IPV4_PATTERN.IsMatch(data);
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "IPv4";
        }
    }
}

