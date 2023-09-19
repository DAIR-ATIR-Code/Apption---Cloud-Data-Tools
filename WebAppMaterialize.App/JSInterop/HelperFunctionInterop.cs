
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.JSInterop;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WebAppMaterialize.App
{
    public static class HelperFunctionInterop
    {
        public static async Task InitializeHighlightJS(this ElementReference elementRef, IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeAsync<object>("HelperFunctionInterop.highlightJS", elementRef);
        }
    }
}

