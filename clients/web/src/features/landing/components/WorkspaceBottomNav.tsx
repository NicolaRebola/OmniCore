import { workspaceMobileNavItems } from '../data/mobile-nav'

export function WorkspaceBottomNav() {
  return (
    <nav className="workspace-bottom-nav" aria-label="Navegación móvil">
      {workspaceMobileNavItems.map((item) => (
        <a
          key={item.id}
          className={
            item.active
              ? 'workspace-bottom-nav__item workspace-bottom-nav__item--active'
              : 'workspace-bottom-nav__item'
          }
          href="#"
          aria-current={item.active ? 'page' : undefined}
        >
          {item.active ? <span className="workspace-bottom-nav__indicator" aria-hidden="true" /> : null}
          {item.label}
        </a>
      ))}
    </nav>
  )
}
