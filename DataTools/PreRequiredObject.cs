
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Text;

namespace DataTools
{
    public class PreRequiredObject
    {
        public SeparatorType SeparatorType { get; set; } = SeparatorType.Comma;
        public bool HasHeaders { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsBrowser { get; set; } = false;
        public bool IsSeparatorSelected { get; set; } = false;
        public bool IsHasHeaderChanged { get; set; } = false;
        public string ElectronAppPath { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public char CustomerSeparator { get; set; } = '\0';
        public PreRequiredObject() { }
    }
}

