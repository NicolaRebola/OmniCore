import { ActivityPanel } from '../components/ActivityPanel'
import { ModuleCard } from '../components/ModuleCard'
import { WorkspaceHeader } from '../components/WorkspaceHeader'
import { WorkspaceHero } from '../components/WorkspaceHero'
import { WorkspaceSidebar } from '../components/WorkspaceSidebar'
import { workspaceModules } from '../data/modules'
import './LandingPage.css'

export function LandingPage() {
  return (
    <div className="workspace-shell" data-testid="landing-page">
      <WorkspaceSidebar />

      <div className="workspace-main">
        <WorkspaceHeader />
        <WorkspaceHero />

        <div className="workspace-body">
          <section className="workspace-modules" aria-labelledby="modules-heading">
            <header className="workspace-modules__header">
              <h2 id="modules-heading">Módulos disponibles</h2>
              <p>
                Por ahora se muestran todos los módulos como habilitados hasta conectar el core
                multi-tenant.
              </p>
            </header>
            <div className="workspace-modules__grid">
              {workspaceModules.map((module) => (
                <ModuleCard key={module.id} module={module} />
              ))}
            </div>
          </section>

          <ActivityPanel />
        </div>
      </div>
    </div>
  )
}
