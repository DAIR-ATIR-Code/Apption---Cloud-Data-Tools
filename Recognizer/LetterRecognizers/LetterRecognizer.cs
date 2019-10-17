
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
    //[StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVachar })]
    public class LetterRecognizer : Recognizer, ILetterRecognizer, IShortStringRecognizer, IMediumStringRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;

        private int minLength = 0, maxLength = 0;

        //private bool isUnicode = false;
        public LetterRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            if (data.Length > maxLength)
                maxLength = data.Length;
            if (data.Length < minLength)
                minLength = data.Length;
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                return StringHelper.IsLetterOnly(data);
            }
            return false;
        }

        public override StorageType[] GetStorageTypes()
        {
            if ((minLength == maxLength && minLength > 0) || (maxLength < 5))
                return new[] { StorageType.Char };
            return new[] { StorageType.Varchar };
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Text";
        }

    }
}

