type MarcadorProps = {
  titulo: string
  descripcion: string
}

export function Marcador({ titulo, descripcion }: MarcadorProps) {
  return (
    <section className="rounded-lg border border-dashed border-slate-300 bg-white p-8">
      <h1 className="text-lg font-semibold tracking-tight text-slate-900">{titulo}</h1>
      <p className="mt-1 text-sm text-slate-600">{descripcion}</p>
    </section>
  )
}
