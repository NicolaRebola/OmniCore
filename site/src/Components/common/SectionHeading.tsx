type SectionHeadingProps = {
  eyebrow: string
  title: string
  body?: string
  wide?: boolean
}

function SectionHeading({ eyebrow, title, body, wide = false }: SectionHeadingProps) {
  return (
    <div className={wide ? 'section-heading section-heading--wide' : 'section-heading'}>
      <p className="section-kicker">{eyebrow}</p>
      <h2>{title}</h2>
      {body ? <p>{body}</p> : null}
    </div>
  )
}

export default SectionHeading
