
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DTHelperStd;
using DataTools;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class PhoneRecognizer : Recognizer, INumberRecognizer, ILongStringRecognizer, ISensitiveRecognizer
    {
        private ColumnMetadata metadata;

        private const int MAX_LENGTH = 20;
        public PhoneRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        //Canada and US only now
        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                return RegexHelper.PHONE_PATTERN.IsMatch(data);
            }
            return false;

            //string localAreaCode = String.Empty;
            //Match match = Regex.Match(localAreaCode, @"^\+(\d{1,3})[\(\-\ ]");
            //if (match.Success)
            //    localAreaCode = match.Value;
            //if (Path.GetFileName(Environment.CurrentDirectory) == ".bin")
            //    databaseFilePath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\..\Recognizer\Resources\reference-data.db");

            //if (AreaCodeList.Contains(localAreaCode))
            //    isPhone++;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Phone";
        }
    }
}

