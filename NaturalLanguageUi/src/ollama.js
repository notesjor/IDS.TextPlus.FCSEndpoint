import { LEXCQL_SCHEMA, SYSTEM_PROMPT, render } from './lexcql.js'

export const MODEL = 'qwen2.5-coder:0.5b'
export const DEFAULT_URL = import.meta.env.VITE_OLLAMA_URL || 'http://localhost:11434'

export async function translate(text, baseUrl = DEFAULT_URL) {
  const url = baseUrl.trim().replace(/\/+$/, '') + '/api/chat'
  const warnings = []
  let res
  try {
    res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        model: MODEL,
        stream: false,
        format: LEXCQL_SCHEMA,
        options: { temperature: 0 },
        messages: [
          { role: 'system', content: SYSTEM_PROMPT },
          { role: 'user', content: text },
        ],
      }),
    })
  } catch (e) {
    throw new Error(`Ollama nicht erreichbar (${url}). Läuft der Server und ist OLLAMA_ORIGINS gesetzt? ${e.message}`)
  }
  if (!res.ok) throw new Error(`Ollama-Fehler ${res.status}: ${await res.text()}`)
  const output = (await res.json())?.message?.content ?? ''
  let args = null
  try {
    args = JSON.parse(output)
  } catch {
    warnings.push('Das Modell hat kein gültiges JSON geliefert.')
    return { query: '', output, args, warnings }
  }
  return { query: render(args, warnings), output, args, warnings }
}
