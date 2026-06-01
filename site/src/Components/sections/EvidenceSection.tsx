import SectionHeading from '../common/SectionHeading'
import type { SiteCopy } from '../../types/site'

type EvidenceSectionProps = {
  evidence: SiteCopy['evidence']
  title: string
}

function EvidenceSection({ evidence, title }: EvidenceSectionProps) {
  return (
    <section className="section section--soft" id="evidence">
      <div className="container">
        <SectionHeading eyebrow="06 / Evidence" title={title} />
        <div className="evidence-grid">
          {evidence.map((item) => (
            <a className="evidence-card" href={item.href} key={item.title}>
              <h3>{item.title}</h3>
              <p>{item.body}</p>
            </a>
          ))}
        </div>
      </div>
    </section>
  )
}

export default EvidenceSection
