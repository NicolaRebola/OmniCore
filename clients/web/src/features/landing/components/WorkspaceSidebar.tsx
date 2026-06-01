import { workspaceNavItems } from '../data/navigation'
import { workspaceTenant } from '../data/tenant'

export function WorkspaceSidebar() {
  return (
    <aside className="workspace-sidebar" aria-label="Navegación principal">
      <div className="workspace-brand">
        <span className="workspace-brand__accent" aria-hidden="true" />
        <div>
          <strong>OmniCore</strong>
          <span>Management OS</span>
        </div>
      </div>

      <nav className="workspace-nav">
        {workspaceNavItems.map((item) => (
          <a
            key={item.id}
            className={item.active ? 'workspace-nav__item workspace-nav__item--active' : 'workspace-nav__item'}
            href="#"
            aria-current={item.active ? 'page' : undefined}
          >
            <span className="workspace-nav__dot" aria-hidden="true" />
            {item.label}
          </a>
        ))}
      </nav>

      <div className="workspace-tenant-card">
        <span className="workspace-tenant-card__label">Tenant activo</span>
        <strong>{workspaceTenant.name}</strong>
        <p>{workspaceTenant.helpText}</p>
      </div>
    </aside>
  )
}
