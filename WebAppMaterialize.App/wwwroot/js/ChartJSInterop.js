window.ChartJSInterop = {
    BarChartReference: undefined,
    PieChartReference: undefined,
    SecondPassResultBarChartRef: undefined,
    RadarChartReference: undefined,
    ProbabilityBarChartRef: undefined,

    InitializeBarChart: function (elementId, barLabels, barData) {

        if (window.ChartJSInterop.BarChartReference) {
            window.ChartJSInterop.BarChartReference.destroy();
        }

        let ctx = document.getElementById(elementId).getContext('2d');
        window.ChartJSInterop.BarChartReference = new Chart(ctx, {
            type: 'bar',            
            data: {
                labels: barLabels,
                datasets: [{
                    backgroundColor: 'rgb(239, 108, 0)',
                    borderColor: 'rgb(214, 96, 0)',
                    data: barData
                }]
            },
            options: {
                legend: {
                    display: false
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                        },
                        scaleLabel: {
                            display: true,
                            labelString: 'Frequence'
                        }
                    }],
                    xAxes: [{
                        ticks: {
                            callback: function (value) {
                                if (value % 5 === 0) {
                                    return value;
                                }
                                return '';
                            }
                        },
                        scaleLabel: {
                            display: true,
                            labelString: 'Length'
                        }
                    }]
                }
            }
        });

        return true;
    },

    InitializePieChart: function (elementId, totalNotNull, totalNull)
    {
        if (window.ChartJSInterop.PieChartReference) {
            window.ChartJSInterop.PieChartReference.destroy();
        }
        var ctx = document.getElementById(elementId).getContext('2d');
        window.ChartJSInterop.PieChartReference = new Chart(ctx, {
            type: 'doughnut',
            data: {
                datasets: [{
                    label: 'Count',
                    data: [totalNotNull, totalNull],
                    backgroundColor: ['rgba(132, 237, 111, 0.8)', 'rgba(239, 44, 14, 0.8)'],
                    borderColor: ['rgb(132, 237, 111)', 'rgb(239, 44, 14)'],
                    borderWidth: 1,
                }],

                labels: [
                    'Data',
                    'Empty'
                ]
            },
            options: {
                title: {
                    display: true,
                    fontSize: 30,
                    text: 'Data Completion'
                },
                legend: {
                    position: 'right',
                }
            }
        });

        return true;

    },

    InitializeSecondPassResultBarChart: function (elementId, title, barData)
    {
        if (window.ChartJSInterop.SecondPassResultBarChartRef) {
            window.ChartJSInterop.SecondPassResultBarChartRef.destroy();
        }
        var ctx = document.getElementById(elementId).getContext('2d');
        var yData = [];
        var xLabel = [];
        var datas = {};
        var options = {};
        switch (barData.graphType) {
            //string length distributions
            case 0:               
                for (var i = barData.minimunX; i < barData.maximunX; i++) {
                    xLabel.push(i);
                }
                yData = Array.apply(null, Array(barData.maximunX - barData.minimunX)).map(Number.prototype.valueOf, 0);
                var dataPoints = barData.dataPoints;
                for (var i = 0; i < dataPoints.length; i++) {
                    yData[dataPoints[i].item1 - barData.minimunX] = dataPoints[i].item2;
                }
                datas = {
                    labels: xLabel,
                    datasets: [{
                        label: 'Frequency',
                        backgroundColor: 'rgba(75, 192, 192, 0.5)',
                        borderColor: 'rgb(75, 192, 192)',
                        borderWidth: 1,
                        data: yData
                    }, {
                        type: 'line',
                        data: yData,
                        fill: false,
                        borderColor: '#EC932F',
                        backgroundColor: '#EC932F',
                    }]
                }
                options = {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        fontSize: 20,
                        text: title
                    },
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,
                            },
                            scaleLabel: {
                                display: true,
                                labelString: barData.yLabel
                            }
                        }],
                        xAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: barData.xLabel
                            },
                            barPercentage: 1.0,
                        }]
                    }
                }
                break;
            //number value distribution
            case 1:
                const BUCKET_SIZE = 10;
                var bucketRange = Math.ceil((barData.maximunX - barData.minimunX) / BUCKET_SIZE);
                for (var i = barData.minimunX; i < barData.maximunX; i += bucketRange) {
                    xLabel.push(i+bucketRange/2);
                }
                yData = Array.apply(null, Array(BUCKET_SIZE)).map(Number.prototype.valueOf, 0);
                var dataPoints = barData.dataPoints;
                for (var i = 0; i < dataPoints.length-1; i += 2) {
                    yData[dataPoints[i].item1 / bucketRange] = dataPoints[i].item2 + dataPoints[i+1].item2;
                }
                datas = {
                    labels: xLabel,
                    datasets: [{
                        label: 'Frequency',
                        backgroundColor: 'rgba(75, 192, 192, 0.5)',
                        borderColor: 'rgb(75, 192, 192)',
                        borderWidth: 1,
                        data: yData
                    }, {
                        type: 'line',
                        data: yData,
                        fill: false,
                        borderColor: '#EC932F',
                        backgroundColor: '#EC932F',
                    }]
                },
                options = {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        fontSize: 20,
                        text: title
                    },
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,
                            },
                            scaleLabel: {
                                display: true,
                                labelString: barData.yLabel
                            }
                        }],
                        xAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: barData.xLabel
                            },
                            barPercentage: 1.0,
                        }]
                    }
                }
                break;
        }
        

        window.ChartJSInterop.SecondPassResultBarChartRef = new Chart(ctx, {
            type: 'bar',
            data: datas,
            options: options,
        });
        return true;
    },

    InitializeRadarChart: function (elementId, probDictionary) {
        if (window.ChartJSInterop.RadarChartReference) {
            window.ChartJSInterop.RadarChartReference.destroy();
        }
        var ctx = document.getElementById(elementId).getContext('2d');

        var labels = [];
        var dataPoints = [];

        for (var i = 0; i < probDictionary.length; i++) {
            labels[i] = probDictionary[i].item1;
        }

        for (var i = 0; i < probDictionary.length; i++) {
            dataPoints[i] = probDictionary[i].item2;
        }

        window.ChartJSInterop.RadarChartReference = new Chart(ctx, {
            type: 'radar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Probability",
                    backgroundColor: 'rgb(255, 99, 132)',
                    borderColor: 'rgb(214, 96, 0)',
                    data: dataPoints
                }]
            },
            options: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: 'Probability Radar'
                },
                scale: {
                    ticks: {
                        beginAtZero: true,
                        max: 1
                    }
                },
            }
        });
        return true;
    },

    InitializeProbabilityBarChart: function (elementId, dataset1, dataset2) {
        if (window.ChartJSInterop.ProbabilityBarChartRef) {
            window.ChartJSInterop.ProbabilityBarChartRef.destroy();
        }
        var ctx = document.getElementById(elementId).getContext('2d');

        var label = [];
        for (var i = 0; i < dataset1.length; i++) {
            label.push(dataset1[i].item1);
        }
        var data1 = [];
        for (var i = 0; i < dataset1.length; i++) {
            data1.push((dataset1[i].item2 * 100).toFixed(2));
        }

        var data2 = [];
        for (var i = 0; i < dataset2.length; i++) {
            if (dataset2[i].item1 == dataset1[i].item1) {
                data2.push((dataset2[i].item2 * 100).toFixed(2));
            }
            else {
                data2.push(0);
            }
        }
        var color = Chart.helpers.color;
        var horizontalBarCharData = {
            labels: label,
            backgroundColor: color(window.chartColors.red).alpha(0.5).rgbString(),
            borderColor: window.chartColors.red,
            borderWidth: 1,
            datasets: [{
                label: 'Probability With Null',
                backgroundColor: color(window.chartColors.red).alpha(0.5).rgbString(),
                borderColor: window.chartColors.red,
                data: data1,
                borderWidth: 1,
            }, {
                label: 'Probability Without Null',
                backgroundColor: color(window.chartColors.blue).alpha(0.5).rgbString(),
                borderColor: window.chartColors.blue,
                data: data2,
                borderWidth: 1,
            }]
        };

        window.ChartJSInterop.ProbabilityBarChartRef = new Chart(ctx, {
            type: 'horizontalBar',
            data: horizontalBarCharData,
            options: {
                legend: {
                    display: 'right'
                },
                title: {
                    display: true,
                    text: 'Probability Result'
                },
                responsive: true,
                scales: {
                    xAxes: [{
                        ticks: {
                            beginAtZero: true,
                            stepSize: 10,
                            max: 100,
                            callback: function (value, index, values) {
                                return value + '%';
                            }
                        },
                        scaleLabel: {
                            display: true,
                            labelString: 'Probability'
                        }
                    }],
                }
            }
        });
        return true;
    },

    Cleanup: function () {
        if (window.ChartJSInterop.PieChartReference)
            window.ChartJSInterop.PieChartReference.destroy();
        if (window.ChartJSInterop.SecondPassResultBarChartRef)
            window.ChartJSInterop.SecondPassResultBarChartRef.destroy();
        if (window.ChartJSInterop.RadarChartReference)
            window.ChartJSInterop.RadarChartReference.destroy();
        if (window.ChartJSInterop.ProbabilityBarChartRef)
            window.ChartJSInterop.ProbabilityBarChartRef.destroy();
    },
}

window.chartColors = {
    red: 'rgb(255, 99, 132)',
    orange: 'rgb(255, 159, 64)',
    yellow: 'rgb(255, 205, 86)',
    green: 'rgb(75, 192, 192)',
    blue: 'rgb(54, 162, 235)',
    purple: 'rgb(153, 102, 255)',
    grey: 'rgb(201, 203, 207)'
};