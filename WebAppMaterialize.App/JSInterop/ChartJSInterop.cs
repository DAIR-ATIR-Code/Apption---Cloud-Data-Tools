
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecognizerTools;
using DataTools;
using System;
using System.Collections.Concurrent;
using NLog;

namespace WebAppMaterialize.App
{    

    public class ChartJSInterop
    {
        //public static Task InitializeBarChart(string canvasId, List<string> labels, List<int> data)
        //{
        //    return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.InitializeBarChart", canvasId, labels, data);
        //}
        private readonly IJSRuntime _jsRuntime;

        public ChartJSInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<object> InitializePieChart(string canvasId, int totalNotNUll, int totalNull)
        {
            return await _jsRuntime.InvokeAsync<object>("ChartJSInterop.InitializePieChart", canvasId, totalNotNUll, totalNull);
        }
        

        public async Task<object> InitializeSecondPassResultBarChart(string canvasId, string title,  RecognizerSummary graphData)
        {
            return await _jsRuntime.InvokeAsync<object>("ChartJSInterop.InitializeSecondPassResultBarChart", canvasId, title, graphData);
        }

        public async Task<object> InitializeProbabilityBarChart(string canvasId, List<(string, float)> dataset1, List<(string, float)> dataset2)
        {
            return await _jsRuntime.InvokeAsync<object>("ChartJSInterop.InitializeProbabilityBarChart", canvasId, dataset1, dataset2);
        }


        public async Task CleanupChart()
        {
            await _jsRuntime.InvokeAsync<object>("ChartJSInterop.Cleanup");
        }
    }
}

