
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DTHelperStd;
using DataTools;
using System;

namespace RecognizerTools
{
    //Specify your StorageTypes for clouds
    [StorageTypes(new[] { StorageType.Varchar, StorageType.Char, StorageType.Nchar, StorageType.NVarchar })]
    //Specify your Charactoristic of your DataType by impletementing a different interface (Support multiple interfaces)
    public class TemplateRecognizer : Recognizer, INumberRecognizer, ILongStringRecognizer, ISensitiveRecognizer
    {
        private ColumnMetadata metadata;

        public TemplateRecognizer(ColumnMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override bool IsMatch(string data, CancellationToken cancellationToken)
        {
            //Collect your data information for the graph displayed
            //By default, it collects the length of the data
            IncrementStats(data.Length);

            if (!cancellationToken.IsCancellationRequested)
            {
                //validataData the data
                return ValidateData(data);
            }
            return false;
        }

        //Put your recognizer's algorithm here
        private bool ValidateData(string data)
        {
            throw new NotImplementedException();
        }

        //Customize your graph detail (Look into MoneyRecognizer for more information)
        public override RecognizerSummary GetStatus()
        {
            return new RecognizerSummary() { DataPoints = GetSummaryStatistics(), XLabel = "Length", YLabel = "Frequency", MaximunX = metadata.MaxLength +2, MinimunX = metadata.MinLengthExceptNull == 0 ? 0 : metadata.MinLengthExceptNull };
        }

        public override string GetDescription()
        {
            //Change to your dataType name
            return "Template";
        }
    }
}

