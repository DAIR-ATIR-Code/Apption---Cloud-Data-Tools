
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using DTHelperStd;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Money, StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]

    public class MoneyRecognizer : Recognizer, INumberRecognizer, IShortStringRecognizer, IMediumStringRecognizer
    {
        private ColumnMetadata metadata;
        private static int BUCKET_SIZE = 20;

        private int[] NumberBucket = Enumerable.Repeat(0, BUCKET_SIZE).ToArray();
        private double BucketRange = 0;

        private const int MAX_LENGTH = 10;
        public MoneyRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
            if (metadata.MaxValue != null && metadata.MinValue != null)
                BucketRange = Math.Ceiling((float)(metadata.MaxValue - metadata.MinValue) / BUCKET_SIZE);
        }

        //Able to find the money without the abbreviations
        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IncrementStats(data.Length);
                if (RegexHelper.MONEY_PATTERN1.IsMatch(data) || RegexHelper.MONEY_PATTERN2.IsMatch(data) || RegexHelper.MONEY_PATTERN3.IsMatch(data))
                {
                    //Need to specify the culture
                    if (!float.TryParse(data, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-CA"), out float result))
                        float.TryParse(data, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-GB"), out result);
                    if(metadata.MaxValue !=null && metadata.MinValue != null)
                    {
                        PlaceIntoBucket(result);
                    }
                    return true;
                }
            }
            return false;
        }

        private void PlaceIntoBucket(float data)
        {

            int bucketIndex =(int)((data - metadata.MinValue) / BucketRange);
            if(data == metadata.MaxValue)
            {
                bucketIndex = BUCKET_SIZE - 1;
            }
            Interlocked.Increment(ref NumberBucket[bucketIndex]);
        }

        public override RecognizerSummary GetStatus()
        {
            if (metadata.MinValue != null && metadata.MaxValue != null)
            {
                var dataPoints = NumberBucket.Select((count, index) => ((float)(index * BucketRange), (float)count)).Where(tp => tp.Item2 > 0).ToList();

                return new RecognizerSummary() { DataPoints = dataPoints, XLabel = "Value", YLabel = "Frequency", MaximunX = (int)metadata.MaxValue, MinimunX = (int)metadata.MinValue, GraphType = GraphType.NumberValue };
            }
            else
            {
                return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength, GraphType = GraphType.StringLength };              
            }
        }

        public override string GetDescription()
        {
            return "Money";
        }
    }
}

