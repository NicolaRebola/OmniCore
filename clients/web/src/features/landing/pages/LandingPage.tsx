import { workspaceActivity } from '../data/activity'
import { workspaceHeroStats } from '../data/hero'
import { workspaceModules } from '../data/modules'
import { workspaceNavItems } from '../data/navigation'
import { workspaceTenant } from '../data/tenant'

/**
 * Placeholder de la landing workspace.
 * El layout visual se implementa en el commit 2.
 */
export function LandingPage() {
  return (
    <main className="landing-page" data-testid="landing-page">
      <header>
        <p>Inicio / Workspace</p>
        <h1>Centro de operaciones</h1>
        <p>{workspaceTenant.name}</p>
      </header>

      <section aria-label="Módulos disponibles">
        <h2>Módulos disponibles</h2>
        <ul>
          {workspaceModules.map((module) => (
            <li key={module.id}>
              {module.title} — {module.metric}
              {module.enabled ? ' (habilitado)' : ''}
            </li>
          ))}
        </ul>
      </section>

      <section aria-label="Actividad reciente">
        <h2>Actividad reciente</h2>
        <ul>
          {workspaceActivity.map((item) => (
            <li key={item.id}>
              {item.label}: {item.title} ({item.timeAgo})
            </li>
          ))}
        </ul>
      </section>

      <section aria-label="Resumen operativo">
        <h2>Resumen</h2>
        <ul>
          {workspaceHeroStats.map((stat) => (
            <li key={stat.label}>
              {stat.label}: {stat.value}
            </li>
          ))}
        </ul>
      </section>

      <nav aria-label="Navegación workspace">
        <ul>
          {workspaceNavItems.map((item) => (
            <li key={item.id}>
              {item.label}
              {item.active ? ' (activo)' : ''}
            </li>
          ))}
        </ul>
      </nav>
    </main>
  )
}
