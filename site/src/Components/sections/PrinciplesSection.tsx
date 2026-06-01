import SectionHeading from '../common/SectionHeading'
import type { SiteCopy } from '../../types/site'

type PrinciplesSectionProps = {
  copy: SiteCopy['principles']
}

function PrinciplesSection({ copy }: PrinciplesSectionProps) {
  return (
    <section className="section" id="principles">
      <div className="container">
        <SectionHeading eyebrow="03 / Principles" title={copy.title} />
        <div className="principles-grid">
          {copy.items.map((principle) => (
            <article className="card" key={principle.title}>
              <h3>{principle.title}</h3>
              <p>{principle.body}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

export default PrinciplesSection
