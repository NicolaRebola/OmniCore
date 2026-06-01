import SectionHeading from '../common/SectionHeading'
import type { SiteCopy } from '../../types/site'

type RoadmapSectionProps = {
  roadmap: SiteCopy['roadmap']
  title: string
}

function RoadmapSection({ roadmap, title }: RoadmapSectionProps) {
  return (
    <section className="section" id="roadmap">
      <div className="container">
        <SectionHeading eyebrow="07 / Roadmap" title={title} />
        <div className="roadmap">
          {roadmap.map((item) => (
            <article className="roadmap-item" key={item.phase}>
              <span>{item.phase}</span>
              <p>{item.body}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

export default RoadmapSection
