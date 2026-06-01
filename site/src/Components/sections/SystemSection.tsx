import SectionHeading from '../common/SectionHeading'
import type { SiteCopy } from '../../types/site'

type SystemSectionProps = {
  copy: SiteCopy['system']
}

function SystemSection({ copy }: SystemSectionProps) {
  return (
    <section className="section section--soft" id="system">
      <div className="container">
        <SectionHeading eyebrow="04 / System" title={copy.title} body={copy.body} wide />
        <div className="system-map">
          {copy.modules.map((module) => (
            <article className="module-card" key={module.name}>
              <div>
                <span className="module-card__status">{module.status}</span>
                <h3>{module.name}</h3>
              </div>
              <p>{module.description}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

export default SystemSection
