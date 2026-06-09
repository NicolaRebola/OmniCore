export type MobileNavItem = {
  id: string
  label: string
  active?: boolean
}

export const workspaceMobileNavItems: MobileNavItem[] = [
  { id: 'home', label: 'Inicio', active: true },
  { id: 'catalog', label: 'Catálogo' },
  { id: 'orders', label: 'Órdenes' },
  { id: 'pos', label: 'POS' },
]
