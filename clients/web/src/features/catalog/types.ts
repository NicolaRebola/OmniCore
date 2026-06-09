export type ViewMode = 'list' | 'grid'

export type ProductStatus = 'Active' | 'Draft' | 'Archived'

export type Product = {
  id: string
  name: string
  sku: string
  category: string
  status: ProductStatus
  variants: number
  updatedAt: string
  inventorySignal: string
}
