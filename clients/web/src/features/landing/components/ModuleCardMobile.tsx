import type { WorkspaceModule } from '../types'

type ModuleCardMobileProps = {
  module: WorkspaceModule
}

export function ModuleCardMobile({ module }: ModuleCardMobileProps) {
  return (
    <article
      className={`module-card-mobile module-card-mobile--${module.accent}`}
      aria-labelledby={`mobile-module-title-${module.id}`}
    >
      <div className={`module-card-mobile__icon module-card-mobile__icon--${module.accent}`}>
        <span className="module-card-mobile__icon-mark" aria-hidden="true" />
        <span className="module-card-mobile__initials">{module.initials}</span>
      </div>
      <div className="module-card-mobile__content">
        <h2 id={`mobile-module-title-${module.id}`}>{module.title}</h2>
        <p>{module.metric}</p>
      </div>
      <button className={`module-card-mobile__action module-card-mobile__action--${module.accent}`} type="button">
        Abrir
      </button>
    </article>
  )
}
