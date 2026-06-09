import { useState } from 'react'
import '../../../App.css'
import { catalogNavItems, products } from '../data/products'
import type { Product, ProductStatus, ViewMode } from '../types'

export function CatalogPage() {
  const [viewMode, setViewMode] = useState<ViewMode>('list')
  const selectedProduct = products[0]

  return (
    <main className="app-shell">
      <aside className="sidebar" aria-label="Main navigation">
        <div className="brand">
          <span className="brand-mark">OC</span>
          <div>
            <strong>OmniCore</strong>
            <span>Management OS</span>
          </div>
        </div>

        <nav className="nav-list">
          {catalogNavItems.map((item) => (
            <a className={item === 'Catalog' ? 'active' : ''} href="#" key={item}>
              {item}
            </a>
          ))}
        </nav>
      </aside>

      <section className="catalog-page">
        <header className="page-header">
          <div>
            <span className="eyebrow">Catalog Service</span>
            <h1>Product catalog</h1>
            <p>
              Create, update and publish catalog items with variants ready for
              inventory, sales and POS flows.
            </p>
          </div>
          <div className="header-actions">
            <button className="button ghost" type="button">
              Export
            </button>
            <button className="button primary" type="button">
              New product
            </button>
          </div>
        </header>

        <section className="metrics" aria-label="Catalog metrics">
          <Metric label="Products" value="248" detail="32 updated this week" tone="primary" />
          <Metric label="Active" value="214" detail="Available for sales" tone="success" />
          <Metric label="Variants" value="612" detail="Sizes, colors, options" tone="secondary" />
          <Metric label="Needs stock" value="18" detail="Inventory module later" tone="warning" />
        </section>

        <section className="toolbar" aria-label="Catalog tools">
          <label className="search-field">
            <span>@</span>
            <input placeholder="Search by product, SKU or variant" />
          </label>
          <select aria-label="Category filter" defaultValue="all">
            <option value="all">All categories</option>
          </select>
          <select aria-label="Status filter" defaultValue="any">
            <option value="any">Any status</option>
          </select>
          <div className="view-toggle" aria-label="View mode">
            <button
              className={viewMode === 'list' ? 'selected' : ''}
              onClick={() => setViewMode('list')}
              type="button"
            >
              List
            </button>
            <button
              className={viewMode === 'grid' ? 'selected' : ''}
              onClick={() => setViewMode('grid')}
              type="button"
            >
              Grid
            </button>
          </div>
          <button className="button ghost" type="button">
            Filters
          </button>
        </section>

        <div className="catalog-content">
          <section className="products-panel" aria-label="Products">
            {viewMode === 'list' ? <ProductTable products={products} /> : <ProductGrid products={products} />}
          </section>

          <aside className="detail-panel" aria-label="Selected product details">
            <span className="eyebrow">Selected product</span>
            <h2>{selectedProduct.name}</h2>
            <p>
              {selectedProduct.sku} · {selectedProduct.category}
            </p>

            <div className="detail-badges">
              <StatusBadge status={selectedProduct.status} />
              <span className="badge info">{selectedProduct.variants} variants</span>
            </div>

            <div className="media-placeholder">Product media</div>

            <section className="inventory-preview">
              <h3>Inventory preview</h3>
              <p>
                Inventory will add stock movements, reorder risk, trends and
                forecast.
              </p>
              <div className="trend-bars" aria-hidden="true">
                {[42, 66, 52, 86, 74, 112, 96].map((height, index) => (
                  <span
                    className={index === 3 ? 'highlight' : ''}
                    key={height + index}
                    style={{ height }}
                  />
                ))}
              </div>
              <strong>Next: feed carts, table orders and POS flows.</strong>
            </section>
          </aside>
        </div>
      </section>
    </main>
  )
}

function Metric({
  label,
  value,
  detail,
  tone,
}: {
  label: string
  value: string
  detail: string
  tone: 'primary' | 'secondary' | 'success' | 'warning'
}) {
  return (
    <article className="metric-card">
      <span className={`metric-mark ${tone}`} />
      <small>{label}</small>
      <strong>{value}</strong>
      <span>{detail}</span>
    </article>
  )
}

function ProductTable({ products }: { products: Product[] }) {
  return (
    <div className="product-table">
      <div className="table-head">
        <span>Product</span>
        <span>SKU</span>
        <span>Category</span>
        <span>Status</span>
        <span>Updated</span>
      </div>
      {products.map((product, index) => (
        <article className={index === 0 ? 'table-row selected' : 'table-row'} key={product.id}>
          <div>
            <strong>{product.name}</strong>
            <small>
              {product.variants} variants · {product.inventorySignal}
            </small>
          </div>
          <span>{product.sku}</span>
          <span>{product.category}</span>
          <StatusBadge status={product.status} />
          <span>{product.updatedAt}</span>
        </article>
      ))}
    </div>
  )
}

function ProductGrid({ products }: { products: Product[] }) {
  return (
    <div className="product-grid">
      {products.map((product, index) => (
        <article className="product-card" key={product.id}>
          <div className={index === 0 ? 'product-media featured' : 'product-media'}>Product media</div>
          <strong>{product.name}</strong>
          <small>
            {product.sku} · {product.category}
          </small>
          <div className="card-actions">
            <StatusBadge status={product.status} />
            <span className="badge info">Variants</span>
            <button className="button secondary compact" type="button">
              Open
            </button>
          </div>
        </article>
      ))}
    </div>
  )
}

function StatusBadge({ status }: { status: ProductStatus }) {
  return <span className={`badge ${status.toLowerCase()}`}>{status}</span>
}
