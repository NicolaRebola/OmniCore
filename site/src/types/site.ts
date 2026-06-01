export type Profile = 'recruiter' | 'developer' | 'executive'

export type Locale = 'es' | 'en'

export type NavCopy = {
  motivation: string
  what: string
  principles: string
  system: string
  decisions: string
  evidence: string
  roadmap: string
  contact: string
}

export type ProfileLabels = Record<Profile, string>

export type SiteCopy = {
  meta: {
    snapshotStatus: string
    snapshotBody: string
    decisionsTitle: string
  }
  nav: NavCopy
  profileHint: string
  profiles: ProfileLabels
  hero: {
    kicker: string
    title: string
    headline: string
    subline: string
    primaryCta: string
    secondaryCta: string
  }
  motivation: {
    title: string
    body: string
    reflection: string
  }
  what: {
    title: string
    body: string
  }
  principles: {
    title: string
    items: Array<{
      title: string
      body: string
    }>
  }
  system: {
    title: string
    body: string
    modules: Array<{
      name: string
      description: string
      status: string
    }>
  }
  decisions: Array<{
    label: string
    title: string
    body: string
  }>
  evidence: Array<{
    title: string
    body: string
    href: string
  }>
  roadmap: Array<{
    phase: string
    body: string
  }>
  closing: {
    eyebrow: string
    title: string
    body: string
  }
}

export type SiteContent = Record<Profile, SiteCopy>
