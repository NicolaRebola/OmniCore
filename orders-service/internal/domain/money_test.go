package domain

import "testing"

func TestNewMoney_Valid(t *testing.T) {
	m, err := NewMoney(1500, "ars")
	if err != nil {
		t.Fatal(err)
	}
	if m.Amount != 1500 {
		t.Fatalf("amount = %d", m.Amount)
	}
	if m.Currency != "ARS" {
		t.Fatalf("currency = %s", m.Currency)
	}
}

func TestNewMoney_Invalid(t *testing.T) {
	tests := []struct {
		name     string
		amount   int64
		currency string
	}{
		{name: "negative amount", amount: -1, currency: "ARS"},
		{name: "empty currency", amount: 100, currency: "  "},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if _, err := NewMoney(tt.amount, tt.currency); err == nil {
				t.Fatal("expected error")
			}
		})
	}
}

func TestMoney_Multiply(t *testing.T) {
	m := mustMoney(t, 1500)
	got := m.Multiply(2)
	if got.Amount != 3000 {
		t.Fatalf("amount = %d", got.Amount)
	}
}

func TestMoney_Add(t *testing.T) {
	a := mustMoney(t, 1000)
	b := mustMoney(t, 500)

	sum, err := a.Add(b)
	if err != nil {
		t.Fatal(err)
	}
	if sum.Amount != 1500 {
		t.Fatalf("amount = %d", sum.Amount)
	}

	otherCurrency, _ := NewMoney(100, "USD")
	if _, err := a.Add(otherCurrency); err == nil {
		t.Fatal("expected currency mismatch error")
	}
}
