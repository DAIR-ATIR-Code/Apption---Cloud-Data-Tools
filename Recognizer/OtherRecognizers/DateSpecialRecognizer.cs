
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Date, StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class DateSpecialRecognizer : Recognizer, ILetterWithNumberRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;

        public List<string> DateFormatTypes { get { return _dateFormatTypes.Except(_invalidDateFormatTypes).ToList(); } }

        private const int MAX_LENGTH = 15;

        private HashSet<string> _dateFormatTypes = new HashSet<string>();
        private HashSet<string> _invalidDateFormatTypes = new HashSet<string>();
        public DateSpecialRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                if (TestDateFormat("dd-MMM-yyyy", data))
                {
                    return true;
                }
                //return DateTime.TryParse(data, out DateTime dateTime);
            }
            return false;              
        }

        private bool TestDateFormat(string dateFormat, string data)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                if (result.Year > 1800)
                {
                    lock (_dateFormatTypes)
                        _dateFormatTypes.Add(dateFormat);
                    return true;
                }
                else
                {
                    lock (_invalidDateFormatTypes)
                        _invalidDateFormatTypes.Add(dateFormat);
                    return false;
                }
            }
            else
            {
                lock (_invalidDateFormatTypes)
                    _invalidDateFormatTypes.Add(dateFormat);
                return false;
            }
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength + 1, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Data (dd-MON-yyyy)";
        }     
    }
}

