import type { SiteCopy } from '../../types/site'

type HeroSectionProps = {
  copy: SiteCopy['hero']
  meta: SiteCopy['meta']
}

function HeroSection({ copy, meta }: HeroSectionProps) {
  return (
    <section className="hero section" id="top">
      <div className="container hero__grid">
        <div className="hero__copy">
          <p className="eyebrow">{copy.kicker}</p>
          <h1>{copy.title}</h1>
          <p className="hero__headline">{copy.headline}</p>
          <p className="hero__subline">{copy.subline}</p>
          <div className="hero__actions" aria-label="Primary actions">
            <a className="button button--primary" href="#motivation">
              {copy.primaryCta}
            </a>
            <a className="button button--ghost" href="https://github.com/nicolarebola/OmniCore">
              {copy.secondaryCta}
            </a>
          </div>
        </div>

        <aside className="hero__note" aria-label="Project snapshot">
          <span className="status-pill">{meta.snapshotStatus}</span>
          <p>{meta.snapshotBody}</p>
        </aside>
      </div>
    </section>
  )
}

export default HeroSection
