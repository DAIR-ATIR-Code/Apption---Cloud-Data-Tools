
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WebAppMaterialize.App
{
    public static class ElementRefExtensions
    {
        public static Task InitializeSelect(this ElementRef elementRef)
        {
            return JSRuntime.Current.InvokeAsync<object>("m_init.select", elementRef);
        }

        public static Task InitializeCollapsible(this ElementRef elementRef)
        {
            return JSRuntime.Current.InvokeAsync<object>("m_init.collapsible", elementRef);
        }

        public static Task InitializeTabs(this ElementRef elementRef)
        {
            return JSRuntime.Current.InvokeAsync<object>("m_init.tabs", elementRef);
        }
     
        public static Task InitializeFloatingAction(this ElementRef elementRef)
        {
            return JSRuntime.Current.InvokeAsync<object>("m_init.floatingActionButton", elementRef);
        }
        public static Task InitializeModal(this ElementRef elementRef)
        {
            return JSRuntime.Current.InvokeAsync<object>("m_init.modal", elementRef);
        }
    }
}

