package domain

import (
	"fmt"
	"strings"
)

const DefaultCurrency = "ARS"

type Money struct {
	Amount   int64 // minor units (centavos)
	Currency string
}

func NewMoney(amount int64, currency string) (Money, error) {
	if amount < 0 {
		return Money{}, fmt.Errorf("money amount must be non-negative")
	}

	currency = strings.TrimSpace(currency)
	if currency == "" {
		return Money{}, fmt.Errorf("money currency is required")
	}

	return Money{
		Amount:   amount,
		Currency: strings.ToUpper(currency),
	}, nil
}

func (m Money) Multiply(qty int) Money {
	return Money{
		Amount:   m.Amount * int64(qty),
		Currency: m.Currency,
	}
}

func (m Money) Add(other Money) (Money, error) {
	if m.Currency != other.Currency {
		return Money{}, fmt.Errorf("cannot add money with different currencies")
	}

	return Money{
		Amount:   m.Amount + other.Amount,
		Currency: m.Currency,
	}, nil
}

func (m Money) IsZero() bool {
	return m.Amount == 0
}
