export type ModuleAccent = 'catalog' | 'orders' | 'pos' | 'finance'

export type WorkspaceModule = {
  id: string
  title: string
  initials: string
  description: string
  metric: string
  accent: ModuleAccent
  enabled: boolean
}

export type WorkspaceActivity = {
  id: string
  label: string
  title: string
  timeAgo: string
  accent: ModuleAccent
}

export type WorkspaceNavItem = {
  id: string
  label: string
  active?: boolean
}

export type WorkspaceTenant = {
  name: string
  helpText: string
}

export type WorkspaceHeroStat = {
  label: string
  value: string
}
