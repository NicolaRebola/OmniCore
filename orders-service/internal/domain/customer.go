package domain

import "strings"

type CustomerSnapshot struct {
	Name  string
	Email string
	Phone string
}

func (c CustomerSnapshot) HasName() bool {
	return strings.TrimSpace(c.Name) != ""
}
