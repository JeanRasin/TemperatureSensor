let seriesChart;
let typeChart = 0;
const intervalDelay = 60000;
const urlApi = "http://localhost:5297/api/Temperature";
//const urlApi = "http://jeanrasin-001-site1.itempurl.com/api/Temperature";

const setTypeChart = function (typeChart) {
    const h1Node = document.querySelector("h1");

    switch (typeChart) {
        case 0:
            seriesChart.set("stroke", am5.color(0x6794dc));
            h1Node.innerText = "График температуры";
            break;

        case 1:
            seriesChart.set("stroke", am5.color(0xff0000));
            h1Node.innerText = "График влажности";
            break;
    }

}

const clickBtnRadioTypeChart = function (e) {
    const forStr = e.currentTarget.htmlFor;

    const inputs = document.querySelectorAll("#btnRadioTypeCharts input");
    for (const input of inputs) {
        input.checked = false;
        input.removeAttribute("checked");
    }

    const inputNode = document.querySelector(`#btnRadioTypeCharts input[name="${forStr}"]`);
    inputNode.setAttribute("checked", "checked");
    inputNode.checked = true;

    setTypeChart(inputNode.id);

    switch (inputNode.id) {
        case "btnRadioTypeChart_0":
            typeChart = 0;
            setTypeChart(typeChart);
            getData(seriesChart, typeChart);
            break;

        case "btnRadioTypeChart_1":
            typeChart = 1;
            setTypeChart(typeChart);
            getData(seriesChart, typeChart);
            break;
    }
}

const btnTypeCharts = document.querySelectorAll('#btnRadioTypeCharts label');
for (const btn of btnTypeCharts) {
    btn.addEventListener("click", clickBtnRadioTypeChart);
}

const getTemperature = function () {

    // Create rootTemperature element
    // https://www.amcharts.com/docs/v5/getting-started/#Root_element
    const rootTemperature = am5.Root.new("chartdiv");

    // Set themes
    // https://www.amcharts.com/docs/v5/concepts/themes/
    rootTemperature.setThemes([
        am5themes_Animated.new(rootTemperature)
    ]);

    // Create chart
    // https://www.amcharts.com/docs/v5/charts/xy-chart/
    const chartTemperature = rootTemperature.container.children.push(am5xy.XYChart.new(rootTemperature, {
        panX: true,
        panY: true,
        wheelX: "panX",
        wheelY: "zoomX",
        pinchZoomX: true
    }));

    chartTemperature.get("colors").set("step", 3);

    // Add cursor
    // https://www.amcharts.com/docs/v5/charts/xy-chart/cursor/
    const cursorTemperature = chartTemperature.set("cursor", am5xy.XYCursor.new(rootTemperature, {}));
    cursorTemperature.lineY.set("visible", false);

    // Create axes
    // https://www.amcharts.com/docs/v5/charts/xy-chart/axes/
    const xAxisTemperature = chartTemperature.xAxes.push(am5xy.DateAxis.new(rootTemperature, {
        maxDeviation: 0.3,
        baseInterval: {
            timeUnit: "minute",
            count: 1
        },
        renderer: am5xy.AxisRendererX.new(rootTemperature, {}),
        tooltip: am5.Tooltip.new(rootTemperature, {})
    }));

    const yAxisTemperature = chartTemperature.yAxes.push(am5xy.ValueAxis.new(rootTemperature, {
        maxDeviation: 0.3,
        renderer: am5xy.AxisRendererY.new(rootTemperature, {})
    }));

    // Add series
    // https://www.amcharts.com/docs/v5/charts/xy-chart/series/
    let seriesTemperature = chartTemperature.series.push(am5xy.LineSeries.new(rootTemperature, {
        name: "Series 1",
        xAxis: xAxisTemperature,
        yAxis: yAxisTemperature,
        valueYField: "value1",
        valueXField: "date",
        tooltip: am5.Tooltip.new(rootTemperature, {
            labelText: "{valueX}: {valueY}\n{previousDate}: {value2}"
        })
    }));

    seriesTemperature.strokes.template.setAll({
        strokeWidth: 2
    });

    seriesTemperature.get("tooltip").get("background").set("fillOpacity", 0.5);

    // Set date fields
    // https://www.amcharts.com/docs/v5/concepts/data/#Parsing_dates
    rootTemperature.dateFormatter.setAll({
        dateFormat: "yyyy-MM-dd HH:MM:ss",
        dateFields: ["valueX"]
    });

    // Make stuff animate on load
    // https://www.amcharts.com/docs/v5/concepts/animations/
    seriesTemperature.appear(1000);

    chartTemperature.appear(1000, 100);

    return seriesTemperature;
}

const getData = function (series, typeChart) {
    fetch(urlApi)
        .then((response) => {
            return response.json();
        })
        .then((data) => {
            console.log(data);

            let temperatureArr = [];
            let humidityArr = [];

            let tbodyStr = "";

            data.forEach(function (item, index, array) {
                temperatureArr.push({
                    date: new Date(item.date).getTime("yyyy-MM-dd HH:MM:ss"),
                    value1: item.temperatureData
                })

                humidityArr.push({
                    date: new Date(item.date).getTime("yyyy-MM-dd HH:MM:ss"),
                    value1: item.humidity
                })

                let dateStr = item.date.replace("T", " ");
                tbodyStr += `
                        <tr>
                            <th>${item.id}</th>
                            <th>${dateStr}</th>
                            <th>${item.temperatureData}</th>
                            <th>${item.humidity}</th>
                        </tr>`
            });

            let tbodyNode = document.querySelector('#temperatureTable tbody')
            tbodyNode.innerHTML = tbodyStr

            switch (typeChart) {
                case 0:
                    series.data.setAll(temperatureArr);
                    break;

                case 1:
                    series.data.setAll(humidityArr);
                    break;
            }

        });
}

const getDataInterval = function () {
    getData(seriesChart, typeChart);

    setInterval(function () {
        setTypeChart(typeChart);
        getData(seriesChart, typeChart);
    }, intervalDelay);
}

const readyChart = function () {
    seriesChart = getTemperature();

    getDataInterval(0);
}

am5.ready(readyChart); // end am5.ready()
