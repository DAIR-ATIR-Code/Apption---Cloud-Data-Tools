
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
    public class ISBNRecognizer : Recognizer, INumberRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;

        private const int MAX_LENGTH = 15;
        public ISBNRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                string temp = RegexHelper.ISBN_PATTERN.Replace(data, "");
                return CheckIsbn(temp);
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "ISBN";
        }

        private static bool CheckIsbn(string isbn)
        {
            if (isbn == null)
                return false;

            if (isbn.Length != 10 && isbn.Length !=13)
                return false;

            int sum = 0;
            if (isbn.Length == 10)
            {
                for (int i = 0; i < 9; i++)
                    sum += (i + 1) * int.Parse(isbn[i].ToString());

                if (sum % 11 == 10)
                    return isbn[9] == 'x' || isbn[9] == 'X';
                return isbn[9] == (char)('0' + sum % 11);

            }else if(isbn.Length == 13)
            {
                for (int i = 0; i < 13; i++)
                {
                    if (i % 2 == 0)
                        sum += int.Parse(isbn[i].ToString());
                    else
                        sum += 3 * int.Parse(isbn[i].ToString());
                }

                return sum % 10 == 0;
            }
           

            return false;
        }


    }
}

