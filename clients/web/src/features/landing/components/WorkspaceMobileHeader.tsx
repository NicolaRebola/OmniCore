import { workspaceModules } from '../data/modules'
import { workspaceTenant } from '../data/tenant'

const enabledCount = workspaceModules.filter((module) => module.enabled).length

export function WorkspaceMobileHeader() {
  return (
    <header className="workspace-mobile-header">
      <div>
        <strong>OmniCore</strong>
        <span>Workspace / {workspaceTenant.name}</span>
      </div>
      <span className="workspace-pill workspace-pill--success">{enabledCount} activos</span>
    </header>
  )
}
