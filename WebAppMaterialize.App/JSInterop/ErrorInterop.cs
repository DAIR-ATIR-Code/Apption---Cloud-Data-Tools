
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
    public static class ErrorInterop
    { 
        public static void Alert(IJSRuntime JSRuntime, string msg)
        {
            JSRuntime.InvokeAsync<object>("alert", msg);
        }
    }
}

