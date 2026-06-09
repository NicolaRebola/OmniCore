import type { WorkspaceActivity } from '../types'

export const workspaceActivity: WorkspaceActivity[] = [
  {
    id: 'activity-catalog',
    label: 'Catálogo',
    title: '12 productos actualizados',
    timeAgo: 'Hace 8 min',
    accent: 'catalog',
  },
  {
    id: 'activity-orders',
    label: 'Comandas',
    title: 'Mesa 04 cambió a listo',
    timeAgo: 'Hace 13 min',
    accent: 'orders',
  },
  {
    id: 'activity-pos',
    label: 'Caja',
    title: 'Pago con tarjeta aprobado',
    timeAgo: 'Hace 21 min',
    accent: 'pos',
  },
  {
    id: 'activity-finance',
    label: 'Finanzas',
    title: 'Reporte diario generado',
    timeAgo: 'Hace 1 h',
    accent: 'finance',
  },
]
