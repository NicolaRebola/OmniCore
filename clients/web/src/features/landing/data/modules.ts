import type { WorkspaceModule } from '../types'

export const workspaceModules: WorkspaceModule[] = [
  {
    id: 'catalog',
    title: 'Catálogo de Productos',
    initials: 'CP',
    description:
      'Administra productos, variantes, precios, categorías e inventario visible para cada canal.',
    metric: '128 productos cargados',
    accent: 'catalog',
    enabled: true,
  },
  {
    id: 'orders',
    title: 'Órdenes y Comandas',
    initials: 'OC',
    description:
      'Centraliza pedidos, comandas de cocina, estados de preparación y seguimiento operativo.',
    metric: '18 órdenes hoy',
    accent: 'orders',
    enabled: true,
  },
  {
    id: 'pos',
    title: 'POS y Caja',
    initials: 'POS',
    description:
      'Gestiona ventas presenciales, turnos de caja, medios de pago y cierres diarios.',
    metric: 'Caja 01 abierta',
    accent: 'pos',
    enabled: true,
  },
  {
    id: 'finance',
    title: 'Finanzas',
    initials: 'FN',
    description:
      'Visualiza ingresos, egresos, conciliaciones y reportes financieros del tenant.',
    metric: '$ 842.300 este mes',
    accent: 'finance',
    enabled: true,
  },
]
