import SectionHeading from '../common/SectionHeading'
import type { SiteCopy } from '../../types/site'

type DecisionsSectionProps = {
  decisions: SiteCopy['decisions']
  title: string
}

function DecisionsSection({ decisions, title }: DecisionsSectionProps) {
  return (
    <section className="section" id="decisions">
      <div className="container">
        <SectionHeading eyebrow="05 / Decisions" title={title} wide />
        <div className="decision-list">
          {decisions.map((decision) => (
            <article className="decision-card" key={decision.title}>
              <span>{decision.label}</span>
              <h3>{decision.title}</h3>
              <p>{decision.body}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

export default DecisionsSection
