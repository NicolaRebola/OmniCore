import { ActivityPanel } from '../components/ActivityPanel'
import { ModuleCard } from '../components/ModuleCard'
import { ModuleCardMobile } from '../components/ModuleCardMobile'
import { WorkspaceBottomNav } from '../components/WorkspaceBottomNav'
import { WorkspaceHeader } from '../components/WorkspaceHeader'
import { WorkspaceHero } from '../components/WorkspaceHero'
import { WorkspaceMobileHeader } from '../components/WorkspaceMobileHeader'
import { WorkspaceMobileIntro } from '../components/WorkspaceMobileIntro'
import { WorkspaceSidebar } from '../components/WorkspaceSidebar'
import { workspaceModules } from '../data/modules'
import './LandingPage.css'

export function LandingPage() {
  return (
    <div className="workspace-shell" data-testid="landing-page">
      <WorkspaceSidebar />

      <div className="workspace-main workspace-desktop-only">
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

      <div className="workspace-mobile workspace-mobile-only">
        <WorkspaceMobileHeader />
        <WorkspaceMobileIntro />
        <section className="workspace-mobile-modules" aria-label="Módulos disponibles">
          {workspaceModules.map((module) => (
            <ModuleCardMobile key={module.id} module={module} />
          ))}
        </section>
        <WorkspaceBottomNav />
      </div>
    </div>
  )
}
