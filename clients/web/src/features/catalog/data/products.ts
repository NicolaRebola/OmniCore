import type { Product } from '../types'

export const products: Product[] = [
  {
    id: 'catalog-001',
    name: 'Organic Coffee Beans',
    sku: 'CAT-001',
    category: 'Beverages',
    status: 'Active',
    variants: 3,
    updatedAt: '2h ago',
    inventorySignal: 'Reorder in 12 days',
  },
  {
    id: 'catalog-014',
    name: 'Ceramic Dinner Plate',
    sku: 'CAT-014',
    category: 'Tableware',
    status: 'Draft',
    variants: 1,
    updatedAt: '1d ago',
    inventorySignal: 'Pending publication',
  },
  {
    id: 'catalog-028',
    name: 'USB-C Hub Pro',
    sku: 'CAT-028',
    category: 'Electronics',
    status: 'Active',
    variants: 4,
    updatedAt: '2d ago',
    inventorySignal: 'Stable demand',
  },
  {
    id: 'catalog-041',
    name: 'House Blend Tea',
    sku: 'CAT-041',
    category: 'Beverages',
    status: 'Active',
    variants: 2,
    updatedAt: '4d ago',
    inventorySignal: 'Trending up',
  },
  {
    id: 'catalog-099',
    name: 'Legacy Gift Card',
    sku: 'CAT-099',
    category: 'Services',
    status: 'Archived',
    variants: 1,
    updatedAt: '12d ago',
    inventorySignal: 'Hidden from sales',
  },
]

export const catalogNavItems = [
  'Dashboard',
  'Catalog',
  'Inventory',
  'Orders',
  'POS',
  'Reports',
  'Settings',
]
