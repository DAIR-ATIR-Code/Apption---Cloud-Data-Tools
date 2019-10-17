
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class SINRecognizer : Recognizer, INumberRecognizer, ILongStringRecognizer, ISensitiveRecognizer
    {
        private ColumnMetadata metadata;

        private const int MAX_LENGTH = 15;
        public SINRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                string temp = data.Replace(" ", "");
                if (!int.TryParse(temp, out int n) || temp.Length != 9)
                    return false;
                return ValidSIN(n);
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength + 1, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "SIN number";
        }

        private bool ValidSIN(int sinNumber)
        {
            int total = 0;

            for (int i = 0; i < 9; i++)
            {
                if (i % 2 == 0)
                {
                    total += sinNumber % 10;
                    sinNumber /= 10;
                }
                else
                {
                    total += sinNumber % 10 >= 5 ? (sinNumber % 10) * 2 - 9 : (sinNumber % 10) * 2;
                    sinNumber /= 10;
                }
            }
            return total % 10 == 0;
        }

    }
}

