
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;

namespace RecognizerTools
{
    public enum GraphType
    {
        StringLength,
        NumberValue
    }

    public class RecognizerSummary
    {
        public const int EMPTY_STRING = 0;
        public const int MAX_LENGTH = 30;

        public GraphType GraphType { get; set; } = GraphType.StringLength;
        public string XLabel { get; set; }
        public string YLabel { get; set; }
        public int MaximunX { get; set; } = MAX_LENGTH;
        public int MinimunX { get; set; } = EMPTY_STRING;

        public List<(float X,float Y)> DataPoints { get; set; }
    }
}
