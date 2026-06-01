import { workspaceModules } from '../data/modules'

const enabledCount = workspaceModules.filter((m) => m.enabled).length

export function WorkspaceHeader() {
  return (
    <header className="workspace-header">
      <div className="workspace-header__titles">
        <span className="workspace-eyebrow">Inicio / Workspace</span>
        <h1>Centro de operaciones</h1>
      </div>
      <div className="workspace-header__actions">
        <span className="workspace-pill workspace-pill--success">
          {enabledCount} módulos habilitados
        </span>
        <button className="workspace-button workspace-button--ghost" type="button">
          Invitar usuario
        </button>
        <button className="workspace-button workspace-button--primary" type="button">
          Configurar
        </button>
      </div>
    </header>
  )
}
