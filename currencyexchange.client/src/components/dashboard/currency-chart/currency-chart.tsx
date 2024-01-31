import { Chart } from "react-google-charts";
import "./currency-chart.css";
import { useEffect, useState } from "react";
import Dropdown from "../../common/dropdown";

type ChartData = {
  currencyCode: string;
  dateToPrice: Currency[];
};

type Currency = {
  key: string;
  value: number;
}

const CurrencyChart: React.FC = () => {
  const [currencyCodesList, setCurrencyCodesList] = useState<string[]>([]);
  const [chartData, setChartData] = useState<[string, any][]>([]);
  const [selectedCurrencyCode, setSelectedCurrencyCode] = useState<string>();
  const chartOptions = {
    hAxis: { gridlines: { color: "#ffffff" } },
    vAxis: { gridlines: { color: "#dddddd" } },
    legend: { position: 'top', alignment: 'center' },
    curveType: "function",
    backgroundColor: 'transparent'
  };

  useEffect(() => {
    fetchAvailableCurrencyCodes();
  }, []);

  function fetchCurrencyData(currencyCode: string) {
    fetch("http://localhost:5132/api/Currency/chartData/" + currencyCode)
    .then((res) => res.json())
    .then((data) => {
      if (data.success) {
        setSelectedCurrencyCode(data.currencyCode);
        updateChartData(data.chartData);
      } else {
        console.error("API call was not successful");
      }
    })
    .catch((err) => {
      console.error(err.message);
    });
  }

  function fetchAvailableCurrencyCodes() {
    fetch("http://localhost:5132/api/Currency/GetAvailableCurrencyCodes/")
    .then((res) => res.json())
    .then((data) => {
      if (data) {
        setCurrencyCodesList(data);
        onCurrencyChange(data[0]);
      } else {
        console.error("API call was not successful");
      }
    })
    .catch((err) => {
      console.error(err.message);
    });
  }

  function updateChartData(chartData: ChartData) {
    const finalData: [string, any][] = [["Date", chartData.currencyCode + '/PLN']];
    chartData.dateToPrice.forEach((dateToPriceSet) => {
      finalData.push([dateToPriceSet.key, dateToPriceSet.value]);
    });

    setChartData(finalData);
  }

  function onCurrencyChange(newCurrencyCode: string) {
    fetchCurrencyData(newCurrencyCode);
  }

  return (
    <div className="currency-chart">
      <div className='chart-title'>Chart view</div>
      <div className="dropdown">
        <Dropdown currencyCodes={currencyCodesList} onDropdownChange={onCurrencyChange} selectedValue={selectedCurrencyCode}></Dropdown>
      </div>
      <div className='chart'>
        <Chart
          chartType="LineChart"
          height="450px"
          width="1000px"
          data={chartData}
          options={chartOptions}
          graph_id="exchangeRatesChart"
        />
      </div>
    </div>
  );
};

export default CurrencyChart;
