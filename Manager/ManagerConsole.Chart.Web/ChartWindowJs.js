function GetInitData() {
    var initData;// = "<InitDataForChart EnableTrendSheetChart=\"True\" HighBid=\"True\" LowBid=\"True\" CurrentDay=\"2014/3/10 14:03:04\" BeginTime=\"2014/3/10 12:03:04\" EndTime=\"2014/3/10 16:03:04\" Language=\"\"><Instruments><Instrument Id=\"af10d803-f811-45d1-a0e3-ddb5bad79ff1\" Description=\"test\" Decimals=\"100\" HasVolume=\"False\"/></Instruments></InitDataForChart>";
    javascript: initData = window.external.GetInitDataForChart();
    return initData;
}

function GetChartQuotation(instrumentId, frequency, fromTime, toTime) {
    var chartQuotations;
    javascript: chartQuotations = window.external.GetChartQuotationInWpf(instrumentId, frequency, fromTime, toTime);
    return chartQuotations;
}

function GetLastQuotationsForTrendSheet() {
    var chart
    javascript: chart = window.external.GetLastQuotationsForTrendSheetFromWpf();
    return chart;
}

function SetRealTimeData(id, time, a, b) {
    try {
        var slHost = document.getElementById("silverlight");
        slHost.content.Page.SetRealTimeData(id, time, a, b);
    } catch (e) {
        alert(e.message);
    }

}

function InitChartWindow(exchangeCode, quotePolicyId) {
    var slHost = document.getElementById("silverlight");
    slHost.content.Page.RealInitailize(exchangeCode, quotePolicyId);
}

function WriteLog(log) {
    javascript: window.external.WriteExperLog(log);
}

function Test(ask) {
    alert("This is Test" + ask);
}