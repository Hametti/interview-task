import React, { useEffect, useState } from 'react';
import './Dropdown.css';

type DropdownProps = {
  currencyCodes: string[];
  selectedValue?: string;
  onDropdownChange: (selectedCurrency: string) => void;
};

const Dropdown: React.FC<DropdownProps> = ({ currencyCodes, selectedValue, onDropdownChange }) => {
  const [selectedCurrency, setSelectedCurrency] = useState(currencyCodes[0] || '');

  useEffect(() => {
    setSelectedCurrency(selectedValue || '');
  }, [selectedValue]);

  const handleDropdownChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedCurrency(event.target.value);
    onDropdownChange(event.target.value);
  };

  return (
    <div className="dropdown-container">
      <select id="currencyDropdown" value={selectedCurrency} onChange={handleDropdownChange}>
        {currencyCodes.map((code) => (
          <option key={code} value={code}>
            {code}
          </option>
        ))}
      </select>
    </div>
  );
}

export default Dropdown;