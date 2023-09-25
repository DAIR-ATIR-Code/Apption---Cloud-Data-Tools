
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
		private IJSRuntime _jSRuntime;

		public NLogJSInteropTarget(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }
        protected override void Write(LogEventInfo logEvent)
        {
            if (_pauseLogging)
            {
                //check early for performance
                return;
            }
			Task.Run(async () => await WriteToOutput(RenderLogEvent(Layout, logEvent)));
			
        }

        private async Task WriteToOutput(string v)
        {
            await _jSRuntime.InvokeAsync<object>("log", v);
        }
    }
}

