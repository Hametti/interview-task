import "./dashboard.css";
import CurrencyChart from "./currency-chart/currency-chart";
import ExchangeRates from "./exchange-rates/exchange-rates";
import CurrencyConverter from "./currency-converter/currency-converter";
import { useEffect, useState } from "react";

const fetchInitialDataEndpoint = 'http://localhost:5132/api/Currency/fetchDataFromNbp'

const Dashboard: React.FC = () => {
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    fetchAvailableCurrencyCodes();
  }, []);

  function fetchAvailableCurrencyCodes() {
    fetch(fetchInitialDataEndpoint)
    .then((data) => {
      if (data.status == 200) {
        setIsLoading(false);
      } else {
        setIsLoading(false);
        console.error("API call was not successful");
      }
    })
    .catch((err) => {
      console.error(err.message);
    });
  }

  return (
    <>
      {!isLoading && <div className="dashboard">
        <div className="left-dashboard-side">
          <CurrencyChart></CurrencyChart>
          <CurrencyConverter></CurrencyConverter>
        </div>
        <div className="right-dashboard-side">
          <ExchangeRates></ExchangeRates>
        </div>
      </div>}
      {isLoading && <div className="loading-container">
        <div className="loader"></div>
        <p>Loading data...</p>
      </div>
      }
    </>
  );
};

export default Dashboard;
