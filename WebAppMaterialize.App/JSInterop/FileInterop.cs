
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WebAppMaterialize.App
{
    public static class FilesInterop
    {
        public static Task InitializeFileInput()
        {
            return JSRuntime.Current.InvokeAsync<object>("fileInterop.initializeFileInput");
        }
        public static async Task<object> GetFileName()
        {
            return await JSRuntime.Current.InvokeAsync<object>("fileInterop.getFileName");
        }
        
        public static async Task<object> GetFilePath()
        {
            return await JSRuntime.Current.InvokeAsync<object>("fileInterop.getFilePath");
        }
    }
}

