import './Header.css'
import type { Locale, NavCopy, Profile, ProfileLabels } from '../types/site'

type HeaderProps = {
  locale: Locale
  nav: NavCopy
  profile: Profile
  profileHint: string
  profileLabels: ProfileLabels
  onLocaleChange: (locale: Locale) => void
  onProfileChange: (profile: Profile) => void
}

const profiles: Profile[] = ['recruiter', 'developer', 'executive']
const locales: Locale[] = ['es', 'en']

function Header({
  locale,
  nav,
  profile,
  profileHint,
  profileLabels,
  onLocaleChange,
  onProfileChange,
}: HeaderProps) {

  return (
    <header className="header">
      <a className="header__brand" href="#top" aria-label="NR OmniCore home">
        NR/OmniCore
      </a>

      <nav className="header__nav" aria-label="Main navigation">
        <a href="#motivation">{nav.motivation}</a>
        <a href="#what">{nav.what}</a>
        <a href="#principles">{nav.principles}</a>
        <a href="#system">{nav.system}</a>
        <a href="#decisions">{nav.decisions}</a>
      </nav>

      <div className="header__controls" aria-label={profileHint}>
        <div className="segmented segmented--profiles" role="group" aria-label={profileHint}>
          {profiles.map((profileOption) => (
            <button
              aria-pressed={profile === profileOption}
              className={profile === profileOption ? 'is-active' : undefined}
              key={profileOption}
              onClick={() => onProfileChange(profileOption)}
              type="button"
            >
              {profileLabels[profileOption]}
            </button>
          ))}
        </div>

        <div className="segmented" role="group" aria-label="Language">
          {locales.map((localeOption) => (
            <button
              aria-pressed={locale === localeOption}
              className={locale === localeOption ? 'is-active' : undefined}
              key={localeOption}
              onClick={() => onLocaleChange(localeOption)}
              type="button"
            >
              {localeOption.toUpperCase()}
            </button>
          ))}
        </div>
      </div>
    </header>
  )
}

export default Header
