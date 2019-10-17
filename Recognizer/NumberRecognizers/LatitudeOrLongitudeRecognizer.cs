
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Decimal, StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class LatitudeOrLongitudeRecognizer : Recognizer, INumberRecognizer, ILongStringRecognizer, IMediumStringRecognizer
    {
        private ColumnMetadata metadata;

        public LatitudeOrLongitudeRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                if (RegexHelper.LATITUDE_PATTERN1.IsMatch(data))
                    return true;
                else if (RegexHelper.LATITUDE_PATTERN2.IsMatch(data))
                    return true;
                else if (RegexHelper.LATITUDE_PATTERN3.IsMatch(data))
                    return true;
                //N80 00 01 or S80.00.01 or 80 00 01N or N29 30 12.11
                else if (RegexHelper.LATITUDE_PATTERN4.IsMatch(data))
                    return true;
                //E10 00 00 or E10.00.00 or 10.00.00E or 10 10 11.11E
                else if (RegexHelper.LATITUDE_PATTERN5.IsMatch(data))
                    return true;
                //41°25'01"N
                else if (RegexHelper.LATITUDE_PATTERN6.IsMatch(data))
                    return true;
            }
            return false;
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2 };
        }

        public override string GetDescription()
        {
            return "Latitude or Longitude";
        }
    }
}

