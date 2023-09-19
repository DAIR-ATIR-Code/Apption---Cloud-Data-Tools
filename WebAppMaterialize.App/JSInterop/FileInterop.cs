
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WebAppMaterialize.App
{
    public static class FilesInterop
    {

        public static async Task<object> InitializeFileInput(IJSRuntime JSRuntime)
        {
            return await JSRuntime.InvokeAsync<object>("fileInterop.initializeFileInput");
        }
        public static async Task<object> GetFileName(IJSRuntime JSRuntime)
        {
            return await JSRuntime.InvokeAsync<object>("fileInterop.getFileName");
        }
        
        public static async Task<object> GetFilePath(IJSRuntime JSRuntime)
        {
            return await JSRuntime.InvokeAsync<object>("fileInterop.getFilePath");
        }
    }
}

