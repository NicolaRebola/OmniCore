package domain

type Totals struct {
	Subtotal Money
	Total    Money
}

func NewTotals(subtotal Money) Totals {
	return Totals{
		Subtotal: subtotal,
		Total:    subtotal,
	}
}
