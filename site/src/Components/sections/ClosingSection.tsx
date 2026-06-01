import type { SiteCopy } from '../../types/site'

type ClosingSectionProps = {
  copy: SiteCopy['closing']
}

function ClosingSection({ copy }: ClosingSectionProps) {
  return (
    <section className="closing section" id="contact">
      <div className="container">
        <p className="eyebrow">{copy.eyebrow}</p>
        <h2>{copy.title}</h2>
        <p>{copy.body}</p>
      </div>
    </section>
  )
}

export default ClosingSection
