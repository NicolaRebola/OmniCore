import type { WorkspaceModule } from '../types'

type ModuleCardProps = {
  module: WorkspaceModule
}

export function ModuleCard({ module }: ModuleCardProps) {
  return (
    <article
      className={`module-card module-card--${module.accent}`}
      aria-labelledby={`module-title-${module.id}`}
    >
      <div className="module-card__top" aria-hidden="true" />
      <div className="module-card__header">
        <div className={`module-card__icon module-card__icon--${module.accent}`}>
          <span className="module-card__icon-mark" aria-hidden="true" />
          <span className="module-card__initials">{module.initials}</span>
        </div>
        {module.enabled ? (
          <span className="workspace-pill workspace-pill--success module-card__status">Habilitado</span>
        ) : null}
      </div>
      <h3 id={`module-title-${module.id}`} className="module-card__title">
        {module.title}
      </h3>
      <p className="module-card__description">{module.description}</p>
      <footer className="module-card__footer">
        <span className={`module-card__metric module-card__metric--${module.accent}`}>
          {module.metric}
        </span>
        <button className="module-card__action" type="button">
          Abrir módulo
        </button>
      </footer>
    </article>
  )
}
