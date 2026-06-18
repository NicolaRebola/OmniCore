package domain

type CustomerSnapshot struct {
	Name  string
	Email string
	Phone string
}

func (c CustomerSnapshot) HasName() bool {
	return c.Name != ""
}
