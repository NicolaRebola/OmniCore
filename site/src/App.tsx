import { useState } from 'react'
import Header from './Components/Header'
import ClosingSection from './Components/sections/ClosingSection'
import DecisionsSection from './Components/sections/DecisionsSection'
import EvidenceSection from './Components/sections/EvidenceSection'
import HeroSection from './Components/sections/HeroSection'
import PrinciplesSection from './Components/sections/PrinciplesSection'
import RoadmapSection from './Components/sections/RoadmapSection'
import SplitSection from './Components/sections/SplitSection'
import SystemSection from './Components/sections/SystemSection'
import { getSiteCopy } from './content'
import type { Locale, Profile } from './types/site'
import './App.css'

function App() {
  const [profile, setProfile] = useState<Profile>('developer')
  const [locale, setLocale] = useState<Locale>('es')
  const copy = getSiteCopy(locale, profile)

  return (
    <>
      <Header
        locale={locale}
        nav={copy.nav}
        profile={profile}
        profileHint={copy.profileHint}
        profileLabels={copy.profiles}
        onLocaleChange={setLocale}
        onProfileChange={setProfile}
      />

      <main>
        <HeroSection copy={copy.hero} meta={copy.meta} />

        <SplitSection
          body={copy.motivation.body}
          eyebrow="01 / Why"
          id="motivation"
          reflection={copy.motivation.reflection}
          title={copy.motivation.title}
        />

        <SplitSection
          body={copy.what.body}
          eyebrow="02 / What"
          id="what"
          soft
          title={copy.what.title}
        />

        <PrinciplesSection copy={copy.principles} />
        <SystemSection copy={copy.system} />
        <DecisionsSection decisions={copy.decisions} title={copy.meta.decisionsTitle} />
        <EvidenceSection evidence={copy.evidence} title={copy.nav.evidence} />
        <RoadmapSection roadmap={copy.roadmap} title={copy.nav.roadmap} />
        <ClosingSection copy={copy.closing} />
      </main>
    </>
  )
}

export default App
