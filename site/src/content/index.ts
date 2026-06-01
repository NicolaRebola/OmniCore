import enContent from './en.json'
import esContent from './es.json'
import type { Locale, Profile, SiteContent, SiteCopy } from '../types/site'

const contentByLocale: Record<Locale, SiteContent> = {
  es: esContent as SiteContent,
  en: enContent as SiteContent,
}

export function getSiteCopy(locale: Locale, profile: Profile): SiteCopy {
  return contentByLocale[locale][profile]
}
