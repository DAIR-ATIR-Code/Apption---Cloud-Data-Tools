
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WebAppMaterialize.App
{
    public static class ElementRefExtensions
    {
        public static async Task InitializeSelect(this ElementReference elementRef, IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeAsync<object>("m_init.select", elementRef);
        }

        public static async Task InitializeCollapsible(this ElementReference elementRef, IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeAsync<object>("m_init.collapsible", elementRef);
        }

        public static async Task InitializeTabs(this ElementReference elementRef, IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeAsync<object>("m_init.tabs", elementRef);
        }
     
        public static async Task InitializeFloatingAction(this ElementReference elementRef, IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeAsync<object>("m_init.floatingActionButton", elementRef);
        }
        public static async Task InitializeModal(this ElementReference elementRef, IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeAsync<object>("m_init.modal", elementRef);
        }
    }
}

