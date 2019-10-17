
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecognizerTools;


namespace WebAppMaterialize.App
{
    public static class HighlightJSInterop
    {
        public static Task ChangeScheme(string title)
        {
            return JSRuntime.Current.InvokeAsync<object>("HighlightJSInterop.changeScheme", title);
        }
    }
}

