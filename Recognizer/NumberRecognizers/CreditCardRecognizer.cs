
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataTools;
using DTHelperStd;

namespace RecognizerTools
{
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    public class CreditCardRecognizer : Recognizer, INumberRecognizer, ILongStringRecognizer, ISensitiveRecognizer
    {
        private ColumnMetadata metadata;

        private const int MAX_LENGTH = 20;
        public CreditCardRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            IncrementStats(data.Length);
            if (!cancellationToken.IsCancellationRequested)
            {
                var temp = data.Replace(" ", "");
                return RegexHelper.CREDIT_CARD_PATTERN.IsMatch(temp) && ValidCreditCard(temp);                    
            }           
            return false;
        }
      
        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            return "Credit Card";
        }

        private static bool ValidCreditCard(string creditCardNumber)
        {
            //// check whether input string is null or empty
            if (string.IsNullOrEmpty(creditCardNumber))
            {
                return false;
            }

            //// 1.	Starting with the check digit double the value of every other digit 
            //// 2.	If doubling of a number results in a two digits number, add up
            ///   the digits to get a single digit number. This will results in eight single digit numbers                    
            //// 3. Get the sum of the digits
            int sumOfDigits = creditCardNumber.Where((e) => e >= '0' && e <= '9')
                            .Reverse()
                            .Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2))
                            .Sum((e) => e / 10 + e % 10);


            //// If the final sum is divisible by 10, then the credit card number
            //   is valid. If it is not divisible by 10, the number is invalid.            
            return sumOfDigits % 10 == 0;
        }


    }
}

