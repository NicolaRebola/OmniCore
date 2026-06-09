import { workspaceHeroStats } from '../data/hero'

export function WorkspaceHero() {
  return (
    <section className="workspace-hero" aria-label="Resumen del workspace">
      <div className="workspace-hero__glow workspace-hero__glow--indigo" aria-hidden="true" />
      <div className="workspace-hero__glow workspace-hero__glow--cyan" aria-hidden="true" />

      <div className="workspace-hero__content">
        <span className="workspace-hero__eyebrow">Workspace del cliente</span>
        <h2>Todo el negocio en una sola entrada.</h2>
        <p>
          Vista inicial para acceder a los módulos disponibles, monitorear actividad reciente y
          continuar tareas clave del tenant.
        </p>
        <div className="workspace-hero__ctas">
          <button className="workspace-button workspace-button--primary" type="button">
            Explorar módulos
          </button>
          <button className="workspace-button workspace-button--hero-secondary" type="button">
            Ver actividad
          </button>
        </div>
      </div>

      <div className="workspace-hero__stats" aria-label="Indicadores rápidos">
        {workspaceHeroStats.map((stat) => (
          <article key={stat.label} className="workspace-hero-stat">
            <strong>{stat.value}</strong>
            <span>{stat.label}</span>
          </article>
        ))}
      </div>
    </section>
  )
}
