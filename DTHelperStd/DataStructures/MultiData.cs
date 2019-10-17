
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTHelperStd.DataStructures
{
    public class MultiData
    {
        [LoadColumn(0)]
        public string DataValue { get; set; }
        [LoadColumn(1)]
        public float Label { get; set; }
    }
}

