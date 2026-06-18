package domain

type Address struct {
	Street     string
	Number     string
	Apartment  string
	City       string
	References string
}

func (a Address) IsCompleteForDelivery() bool {
	return a.Street != "" && a.Number != "" && a.City != ""
}
