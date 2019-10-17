
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
    [StorageTypes(new [] {  StorageType.Int, StorageType.Decimal, StorageType.Smallint, StorageType.Bigint, StorageType.Varchar, StorageType.NVarchar, StorageType.Char, StorageType.Nchar })]
    public class NumberRecognizer : Recognizer, INumberRecognizer, IShortStringRecognizer, IMediumStringRecognizer, ILongStringRecognizer
    {
        private ColumnMetadata metadata;

        private HashSet<StorageType> _invalidStorageTypes = new HashSet<StorageType>();
        public bool _isNumberWithMinus = false;
        public NumberRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                if (!int.TryParse(data, out int d))
                {
                    lock(_invalidStorageTypes)
                    {
                        _invalidStorageTypes.Add(StorageType.Int);                        
                    }
                }
                //Potential bug: 10,00 will pass.
                if (decimal.TryParse(data, out decimal result))
                {
                    if(!_isNumberWithMinus && result < 0)
                    {
                        lock (this)
                        {
                            _isNumberWithMinus = true;
                        }
                    }
                    return true;
                }
                //    return true;
                //else if (RegexHelper.NUMBER_PATTERN2.IsMatch(data))
                //{
                //    if (!_isNumberWithMinus)
                //    {
                //        lock (this)
                //        {
                //            _isNumberWithMinus = true;
                //        }
                //    }
                //    return true;
                //}
            }
            return false;
        }

        public override StorageType[] GetStorageTypes()
        {
            return base.GetStorageTypes().Except(_invalidStorageTypes).ToArray();
        }

        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +1, MinimunX = metadata.MinLengthExceptNull-1 };
        }

        public override string GetDescription()
        {
            return "Number";
        }

    }
}
