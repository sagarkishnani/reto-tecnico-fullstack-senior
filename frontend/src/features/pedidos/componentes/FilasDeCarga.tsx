export function FilasDeCarga({ columnas, filas = 4 }: { columnas: number; filas?: number }) {
  return (
    <>
      {Array.from({ length: filas }, (_, fila) => (
        <tr key={fila} aria-hidden="true">
          {Array.from({ length: columnas }, (_, columna) => (
            <td key={columna} className="px-4 py-3.5">
              <div className="h-3.5 animate-pulse rounded bg-slate-200" />
            </td>
          ))}
        </tr>
      ))}
    </>
  )
}
