import React, { useEffect, useState } from 'react';
import './exchange-rates.css';

type CurrencyData = {
  currencyName: string;
  currencyCode: string;
  currencyMidValue: number;
  currencyBidValue: number;
  currencyAskValue: number;
}

const ExchangeRatesEndpointUrl: string = 'http://localhost:5132/api/Currency/getExchangeRates';

const ExchangeRates: React.FC = () => {
  const [currencies, setCurrencies] = useState<CurrencyData[]>([]);

  useEffect(() => {
    fetchExchangeRates();
  }, []);

  function fetchExchangeRates() {
    fetch(ExchangeRatesEndpointUrl)
      .then((res) => res.json())
      .then((data) => {
        if (data.success) {
          setCurrencies(data.exchangeRates);
        } else {
          console.error("API call was not successful");
        }
      })
      .catch((err) => {
        console.error(err.message);
      });
  }

  return (
    <div className="exchange-rates">
      <div className='exchange-rates-title'>Exchange rates</div>
      <div className="exchange-rates-header">
        <div>Code</div>
        <div>Name</div>
        <div>Mid rate</div>
        <div>Bid rate</div>
        <div>Ask rate</div>
      </div>
      {currencies.map((currency) => (
        <div className={`currency-row`} key={currency.currencyCode}>
          <div>{currency.currencyCode}</div>
          <div>{currency.currencyName}</div>
          <div>{currency.currencyMidValue.toFixed(2)} PLN</div>
          <div>{currency.currencyBidValue.toFixed(2)} PLN</div>
          <div>{currency.currencyAskValue.toFixed(2)} PLN</div>
        </div>
      ))}
    </div>
  );
};

export default ExchangeRates;
