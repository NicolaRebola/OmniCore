import { workspaceActivity } from '../data/activity'

export function ActivityPanel() {
  return (
    <aside className="activity-panel" aria-label="Actividad reciente">
      <header className="activity-panel__header">
        <h2>Actividad reciente</h2>
        <p>Señales operativas para retomar trabajo rápidamente.</p>
      </header>

      <ul className="activity-panel__list">
        {workspaceActivity.map((item) => (
          <li key={item.id} className="activity-panel__item">
            <span className={`activity-panel__dot activity-panel__dot--${item.accent}`} aria-hidden="true" />
            <div>
              <span className={`activity-panel__label activity-panel__label--${item.accent}`}>
                {item.label}
              </span>
              <strong>{item.title}</strong>
              <time>{item.timeAgo}</time>
            </div>
          </li>
        ))}
      </ul>

      <div className="activity-panel__quick-actions">
        <h3>Acciones rápidas</h3>
        <p>Crear producto, abrir caja o revisar comandas pendientes.</p>
      </div>
    </aside>
  )
}
