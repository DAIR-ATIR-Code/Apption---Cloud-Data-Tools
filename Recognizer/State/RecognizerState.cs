
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RecognizerTools.State
{
    public class RecognizerState
    {
        public const int MAX_STAT_LENGTH = 1000;
        public int Count { get; set; }
        public int[] Statistics { get; set; } = new int[MAX_STAT_LENGTH];

        public RecognizerSummary Summary { get; set; }
        public float Probability { get; set; } = 0;
        public float ProbabilityWithoutNull { get; set; } = 0;
    }
}

