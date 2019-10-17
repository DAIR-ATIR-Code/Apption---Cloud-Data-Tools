
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecognizerTools;
using DataTools;
using System;
using System.Collections.Concurrent;

namespace WebAppMaterialize.App
{    

    public static class ChartJSInterop
    {
        public static Task InitializeBarChart(string canvasId, List<string> labels, List<int> data)
        {
            return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.InitializeBarChart", canvasId, labels, data);
        }

        public static Task InitializePieChart(string canvasId, int totalNotNUll, int totalNull)
        {
            return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.InitializePieChart", canvasId, totalNotNUll, totalNull);
        }
        
        public static Task InitializeSecondPassResultBarChart(string canvasId, string title,  RecognizerSummary graphData)
        {
            return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.InitializeSecondPassResultBarChart", canvasId, title, graphData);
        }

        public static Task InitializeProbabilityBarChart(string canvasId, List<(string, float)> dataset1, List<(string, float)> dataset2)
        {
            return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.InitializeProbabilityBarChart", canvasId, dataset1, dataset2);
        }


        public static Task InitializeRadarChart(string canvasId, List<(string, float)> probList)
        {
            return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.InitializeRadarChart", canvasId, probList);            
        }

        public static Task CleanupChart()
        {
            return JSRuntime.Current.InvokeAsync<object>("ChartJSInterop.Cleanup");
        }
    }
}

