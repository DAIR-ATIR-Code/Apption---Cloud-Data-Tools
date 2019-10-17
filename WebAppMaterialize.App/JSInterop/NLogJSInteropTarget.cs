
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.JSInterop;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppMaterialize.App
{
    [Target("JSInterop")]
    public class NLogJSInteropTarget : TargetWithLayout
    {
        private bool _pauseLogging = false;

        protected override void Write(LogEventInfo logEvent)
        {
            if (_pauseLogging)
            {
                //check early for performance
                return;
            }
            WriteToOutput(RenderLogEvent(Layout, logEvent));
        }

        private void WriteToOutput(string v)
        {
            JSRuntime.Current.InvokeAsync<object>("log", v);
        }
    }
}

