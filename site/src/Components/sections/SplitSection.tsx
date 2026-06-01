type SplitSectionProps = {
  id: string
  eyebrow: string
  title: string
  body: string
  reflection?: string
  soft?: boolean
}

function SplitSection({ id, eyebrow, title, body, reflection, soft = false }: SplitSectionProps) {
  return (
    <section className={soft ? 'section section--soft' : 'section'} id={id}>
      <div className="container split">
        <p className="section-kicker">{eyebrow}</p>
        <div>
          <h2>{title}</h2>
          <p>{body}</p>
          {reflection ? <blockquote>{reflection}</blockquote> : null}
        </div>
      </div>
    </section>
  )
}

export default SplitSection
