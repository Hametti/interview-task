import React, { useEffect, useState } from 'react';
import Dropdown from '../../common/dropdown';
import './currency-converter.css';

type Currency = {
  currencyName: string;
  code: string;
  mid: number;
}

const CurrenciesEndpointUrl: string = 'http://localhost:5132/api/Currency/all';

const CurrencyConverter: React.FC = () => {
  const [sourceCurrencyAmount, setSourceCurrencyAmount] = useState<string>('');
  const [targetCurrencyAmount, setTargetCurrencyAmount] = useState<string>('');
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [currencyCodesList, setCurrencyCodesList] = useState<string[]>([]);
  const [selectedSourceCurrencyCode, setSelectedSourceCurrencyCode] = useState<string>(currencyCodesList[0]);
  const [selectedTargetCurrencyCode, setSelectedTargetCurrencyCode] = useState<string>(currencyCodesList[1]);

  useEffect(() => {
    if (sourceCurrencyAmount != null) {
      recalculateTargetAmount(sourceCurrencyAmount);
    }
  }, [selectedSourceCurrencyCode, selectedTargetCurrencyCode]);

  useEffect(() => {
    fetchCurrencies();
  }, []);

  function fetchCurrencies() {
    fetch(CurrenciesEndpointUrl)
    .then((res) => res.json())
    .then((data) => {
      if (data.success) {
        const plnCurrency = {
          currencyName: 'Polski złoty',
          code: 'PLN',
          mid: 1
        };

        const currenciesFromApi: Currency[] = [plnCurrency, ...data.currencies];
        const currencyCodes = currenciesFromApi.map((currency) => currency.code)
        setCurrencies(currenciesFromApi);
        setCurrencyCodesList(currencyCodes);
        setSelectedSourceCurrencyCode(currencyCodes[0]);
        setSelectedTargetCurrencyCode(currencyCodes[1]);
      } else {
        console.error("API call was not successful");
      }
    })
    .catch((err) => {
      console.error(err.message);
    });
  }

  const handleSourceCurrencyAmountChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newSourceCurrencyAmount = event.target.value;
    setSourceCurrencyAmount(newSourceCurrencyAmount);
    recalculateTargetAmount(newSourceCurrencyAmount);
  };

  const handleTargetCurrencyAmountChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newTargetCurrencyAmount = event.target.value;
    setTargetCurrencyAmount(newTargetCurrencyAmount);
    recalculateSourceAmount(newTargetCurrencyAmount);
  };

  function recalculateSourceAmount(newTargetAmount: string): void {
    const sourceTargetNumber = parseFloat(newTargetAmount);
    if (!isNaN(sourceTargetNumber)) {
      const sourceCurrencyValue = currencies.find((currency) => currency.code === selectedSourceCurrencyCode)?.mid || 0;
      const targetCurrencyValue = currencies.find((currency) => currency.code === selectedTargetCurrencyCode)?.mid || 0;
      setSourceCurrencyAmount((sourceTargetNumber * (targetCurrencyValue / sourceCurrencyValue)).toFixed(3).toString());
      setTargetCurrencyAmount(newTargetAmount);
    }
    if (newTargetAmount == '') {
      resetCurrencyAmounts();
    }
  }

  function recalculateTargetAmount(newSourceAmount: string): void {
    const sourceAmountNumber = parseFloat(newSourceAmount);
    if (!isNaN(sourceAmountNumber)) {
      const sourceCurrencyValue = currencies.find((currency) => currency.code === selectedSourceCurrencyCode)?.mid || 0;
      const targetCurrencyValue = currencies.find((currency) => currency.code === selectedTargetCurrencyCode)?.mid || 0;
      setTargetCurrencyAmount((sourceAmountNumber * (sourceCurrencyValue / targetCurrencyValue)).toFixed(3).toString());
      setSourceCurrencyAmount(newSourceAmount);
    }
    if (newSourceAmount == '') {
      resetCurrencyAmounts();
    }
  }

  function reset(): void {
    resetDropdowns();
    resetCurrencyAmounts();
  }

  function resetDropdowns(): void {
    setSelectedSourceCurrencyCode(currencyCodesList[0] ?? '');
    setSelectedTargetCurrencyCode(currencyCodesList[1] ?? '');
  }

  function resetCurrencyAmounts(): void {
    setSourceCurrencyAmount('');
    setTargetCurrencyAmount('');
  }

  function onSourceCurrencyCodeChange(newSourceCurrencyCode: string) {
    setSelectedSourceCurrencyCode(newSourceCurrencyCode);
  }

  function onTargetCurrencyCodeChange(newTargetCurrencyCode: string) {
    setSelectedTargetCurrencyCode(newTargetCurrencyCode);
  }

  function invertSelectedCurrencies(): void {
    const temp = selectedSourceCurrencyCode;
    setSelectedSourceCurrencyCode(selectedTargetCurrencyCode);
    setSelectedTargetCurrencyCode(temp);
  }

  return (
    <div className="converter">
      <div className="converter-title">Currency converter</div>
      <div className='converter-form'>
        <div className="conversion-direction">
          <div className='dropdown-with-title'>
            <span className='dropdown-title'>From</span>
            <Dropdown currencyCodes={currencyCodesList} selectedValue={selectedSourceCurrencyCode} onDropdownChange={onSourceCurrencyCodeChange}></Dropdown>
          </div>
          <span className='arrows' onClick={invertSelectedCurrencies}>&#8596;</span>
          <div className='dropdown-with-title'>
            <span className='dropdown-title'>To</span>
            <Dropdown currencyCodes={currencyCodesList} selectedValue={selectedTargetCurrencyCode} onDropdownChange={onTargetCurrencyCodeChange}></Dropdown>
          </div>
        </div>
        <div className='input-groups'>
          <div className="input-group">
          <span className='dropdown-title'>Amount [{selectedSourceCurrencyCode}]</span>
            <input
              type="text"
              className="input-field"
              placeholder="Source currency amount"
              value={`${sourceCurrencyAmount}`}
              onInput={handleSourceCurrencyAmountChange}
            />
          </div>
          <div className="input-group">
          <span className='dropdown-title'>Converted to [{selectedTargetCurrencyCode}]</span>

            <input
              type="text"
              className="input-field"
              placeholder="Target currency amount"
              value={`${targetCurrencyAmount}`}
              onChange={handleTargetCurrencyAmountChange}
            />
          </div>
        </div>
        <button className="reset-button" onClick={reset}>Reset</button>
      </div>
    </div>
  );
};

export default CurrencyConverter;