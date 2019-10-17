
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DataTools;
using DTHelperStd;

namespace RecognizerTools
{
    [StorageTypes(new []{ StorageType.Date, StorageType.Varchar})]
    public class DateRecognizer : Recognizer, INumberRecognizer, IMediumStringRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;

        private DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime BiggestTime = DateTime.MinValue;
        private DateTime SmallestTime = DateTime.MaxValue;
        public List<string> DateFormatTypes { get { return _dateFormatTypes.Except(_invalidDateFormatTypes).ToList(); } }

        private const int MAX_LENGTH = 20;
        //the below number means near 9999/12/31 in epoch time
        private const long MAX_TIME = 2000000000000;
        private const long MAX_YEAR = 2200;
        private const long MIN_YEAR= 1970;

        private HashSet<string> _dateFormatTypes = new HashSet<string>();
        private HashSet<string> _invalidDateFormatTypes = new HashSet<string>();
        public DateRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                //DateTime epochTime = new DateTime();
                //if (Double.TryParse(data, out double second))
                //{                    
                //    if (second < MAX_TIME)
                //        epochTime = Epoch.AddSeconds(second);
                //}

                //if (DateTime.TryParse(data, out DateTime dateTime))
                //{
                //    return true;
                //}

                if (RegexHelper.DATE_PATTERN2.IsMatch(data))
                {
                    TestDateFormat("d/M/yyyy", data);
                    TestDateFormat("d-M-yyyy", data);
                    TestDateFormat("d.M.yyyy", data);
                    TestDateFormat("d M yyyy", data);

                    TestDateFormat("dd/MM/yyyy", data);
                    TestDateFormat("dd-MM-yyyy", data);
                    TestDateFormat("dd.MM.yyyy", data);
                    TestDateFormat("dd MM yyyy", data);

                    TestDateFormat("dd/MM/yy", data);
                    TestDateFormat("dd-MM-yy", data);
                    TestDateFormat("dd.MM.yy", data);
                    TestDateFormat("dd MM yy", data);

                    TestDateFormat("d/M/yy", data);
                    TestDateFormat("d-M-yy", data);
                    TestDateFormat("d.M.yy", data);
                    TestDateFormat("d M yy", data);
                    return true;
                }
                
                else if (RegexHelper.DATE_PATTERN1.IsMatch(data))
                {
                    TestDateFormat("M/d/yyyy", data);
                    TestDateFormat("M-d-yyyy", data);
                    TestDateFormat("M.d.yyyy", data);
                    TestDateFormat("M d yyyy", data);

                    TestDateFormat("MM/dd/yyyy", data);
                    TestDateFormat("MM-dd-yyyy", data);
                    TestDateFormat("MM.dd.yyyy", data);
                    TestDateFormat("MM dd yyyy", data);

                    TestDateFormat("MM/dd/yy", data);
                    TestDateFormat("MM-dd-yy", data);
                    TestDateFormat("MM.dd.yy", data);
                    TestDateFormat("MM dd yy", data);

                    TestDateFormat("M/d/yy", data);
                    TestDateFormat("M-d-yy", data);
                    TestDateFormat("M.d.yy", data);
                    TestDateFormat("M d yy", data);
                    return true;
                }

                else if (RegexHelper.DATE_PATTERN5.IsMatch(data))
                {
                    TestDateFormat("yyyy/MM/dd", data);
                    TestDateFormat("yyyy-MM-dd", data);
                    TestDateFormat("yyyy.MM.dd", data);
                    TestDateFormat("yyyy MM dd", data);

                    TestDateFormat("yyyy.M.d", data);
                    TestDateFormat("yyyy-M-d", data);
                    TestDateFormat("yyyy/M/d", data);
                    TestDateFormat("yyyy M d", data);
                    return true;
                }

                //mmddyyyy mmddyy
                else if (RegexHelper.DATE_PATTERN3.IsMatch(data))
                {
                    TestDateFormat("MMddyy", data);
                    TestDateFormat("MMddyyyy", data);
                    return true;
                }

                //yyyyddmm yyyymmdd
                else if (RegexHelper.DATE_PATTERN4.IsMatch(data))
                {
                    TestDateFormat("yyyyddMM", data);
                    TestDateFormat("yyyyMMdd", data);
                    return true;
                }            

                //else if (epochTime.Year < MAX_YEAR && epochTime.Year > MIN_YEAR)
                //    return true;
            }            
            return false;
        }

        private void TestDateFormat(string dateFormat, string data)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                RecordGraphInfo(result);

                if (result.Year > 1800)
                {
                    lock (_dateFormatTypes)
                        _dateFormatTypes.Add(dateFormat);
                }
                else
                {
                    lock (_invalidDateFormatTypes)
                        _invalidDateFormatTypes.Add(dateFormat);
                }
            }
            else
            {
                lock (_invalidDateFormatTypes)
                    _invalidDateFormatTypes.Add(dateFormat);
            }
        }

        private void RecordGraphInfo(DateTime dateTime)
        {
            //maybe useful
            if (BiggestTime.CompareTo(dateTime) < 0)
                BiggestTime = dateTime;
            else if (SmallestTime.CompareTo(dateTime) > 0)
                SmallestTime = dateTime;


            //TODO: IMprove stats
            //if (dateTime.Year > 0)
            //{
            //    stats.AddOrUpdate(dateTime.Year, 1, (y, c) => c + 1);
                
            //}
            //else if (dateTime.Month > 0)
            //    stats.AddOrUpdate(dateTime.Month, 1, (m, c) => c + 1);
            //else if (dateTime.Day > 0)
            //    stats.AddOrUpdate(dateTime.Day, 1, (d, c) => c + 1);
            //else if (dateTime.Hour > 0)
            //    stats.AddOrUpdate(dateTime.Hour, 1, (h, c) => c + 1);
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Date";
        }
    }
}

