package postgres

import (
	"encoding/json"
	"fmt"

	"orders-service/internal/domain"
)

type moneyJSON struct {
	Amount   int64  `json:"amount"`
	Currency string `json:"currency"`
}

func moneyToJSON(m domain.Money) ([]byte, error) {
	return json.Marshal(moneyJSON{Amount: m.Amount, Currency: m.Currency})
}

func moneyFromJSON(data []byte) (domain.Money, error) {
	if len(data) == 0 {
		return domain.Money{}, fmt.Errorf("money json is empty")
	}
	var raw moneyJSON
	if err := json.Unmarshal(data, &raw); err != nil {
		return domain.Money{}, err
	}
	return domain.NewMoney(raw.Amount, raw.Currency)
}

func optionalMoneyToJSON(m *domain.Money) ([]byte, error) {
	if m == nil {
		return nil, nil
	}
	return moneyToJSON(*m)
}

func optionalMoneyFromJSON(data []byte) (*domain.Money, error) {
	if len(data) == 0 {
		return nil, nil
	}
	m, err := moneyFromJSON(data)
	if err != nil {
		return nil, err
	}
	return &m, nil
}

func customerToJSON(c *domain.CustomerSnapshot) ([]byte, error) {
	if c == nil {
		return nil, nil
	}
	return json.Marshal(c)
}

func customerFromJSON(data []byte) (*domain.CustomerSnapshot, error) {
	if len(data) == 0 {
		return nil, nil
	}
	var c domain.CustomerSnapshot
	if err := json.Unmarshal(data, &c); err != nil {
		return nil, err
	}
	return &c, nil
}

func addressToJSON(a *domain.Address) ([]byte, error) {
	if a == nil {
		return nil, nil
	}
	return json.Marshal(a)
}

func addressFromJSON(data []byte) (*domain.Address, error) {
	if len(data) == 0 {
		return nil, nil
	}
	var a domain.Address
	if err := json.Unmarshal(data, &a); err != nil {
		return nil, err
	}
	return &a, nil
}

type totalsJSON struct {
	Subtotal moneyJSON `json:"subtotal"`
	Total    moneyJSON `json:"total"`
}

func totalsToJSON(t *domain.Totals) ([]byte, error) {
	if t == nil {
		return nil, nil
	}
	return json.Marshal(totalsJSON{
		Subtotal: moneyJSON{Amount: t.Subtotal.Amount, Currency: t.Subtotal.Currency},
		Total:    moneyJSON{Amount: t.Total.Amount, Currency: t.Total.Currency},
	})
}

func totalsFromJSON(data []byte) (*domain.Totals, error) {
	if len(data) == 0 {
		return nil, nil
	}
	var raw totalsJSON
	if err := json.Unmarshal(data, &raw); err != nil {
		return nil, err
	}
	subtotal, err := domain.NewMoney(raw.Subtotal.Amount, raw.Subtotal.Currency)
	if err != nil {
		return nil, err
	}
	total, err := domain.NewMoney(raw.Total.Amount, raw.Total.Currency)
	if err != nil {
		return nil, err
	}
	return &domain.Totals{Subtotal: subtotal, Total: total}, nil
}
