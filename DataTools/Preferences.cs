
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Text;

namespace DataTools
{
    public class Preferences
    {
        public bool RoundVarcharSize { get; set; } = true;
        public int SQLTimeoutMinutes { get; set; } = 10;
        public int SQLBatchSize { get; set; }

        public bool UseTempDBFile { get; set; } = true;
        public int VarCharSizeForEmptyColums { get; set; } = 10;
        public bool DropTableIfExists { get; set; } = true;
        public double UIUpdateFrequencySecondPass { get; set; } = 2;
        public bool EnableAddressRecognizer { get; set; } = false;
        public int RecognizerThresHold { get; set; } = 85;
        public string ElectronAppPath { get; set; } = string.Empty;
    }
}

