// Renders structured conditions (model JSON output) into a LexCQL string
// that is accepted by LexCqlTokenizer / LexCqlParser.

export const FIELDS = ['lemma', 'pos', 'definition', 'gender']
export const OPERATORS = ['=', '==', '!=', 'IS']
export const MODIFIERS = ['ignoreCase', 'respectCase', 'regexp']
export const CONNECTORS = ['AND', 'OR']

/** JSON schema for Ollama structured output (`format`). */
export const LEXCQL_SCHEMA = {
  type: 'object',
  properties: {
    conditions: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          field: { type: 'string', enum: FIELDS },
          op: { type: 'string', enum: OPERATORS },
          modifier: { type: 'string', enum: MODIFIERS },
          value: { type: 'string' },
          connector: { type: 'string', enum: CONNECTORS },
          negate: { type: 'boolean' },
          open_paren: { type: 'integer' },
          close_paren: { type: 'integer' },
        },
        required: ['field', 'value'],
      },
    },
  },
  required: ['conditions'],
}

export const SYSTEM_PROMPT = `You translate a natural-language request about dictionary entries into JSON conditions. Answer with JSON only.
Fields: lemma (word form; "*" wildcard only at the start and/or end, e.g. "*in", "Berg*"), pos (UPOS tag: NOUN, VERB, ADJ, ADV, ...), definition (meaning, e.g. Person), gender (Masc, Fem, Neut).
op: "==" (default), "=", "!=" not equal, "IS" (categorical values, e.g. pos IS NOUN). modifier "regexp" means value is a regular expression.
connector ("AND" default, or "OR") joins a condition with the previous one. negate=true adds NOT (do not also add AND before it). open_paren/close_paren (integers) group conditions.
Example: "female nouns ending in -in that refer to a person" ->
{"conditions":[{"field":"lemma","value":"*in"},{"field":"pos","value":"NOUN"},{"field":"definition","value":"Person"},{"field":"gender","value":"Fem"}]}`

const SIMPLE = /^(?:[\p{L}\p{N}_*?']|\\.)+$/u

/** Wildcards are only allowed at the start/end of a word; inner ones are escaped. */
export function normalizeWildcards(value) {
  const lead = value.startsWith('*') ? '*' : ''
  const rest = value.slice(lead.length)
  const trail = rest.length > 0 && rest.endsWith('*') && !rest.endsWith('\\*') ? '*' : ''
  const core = rest.slice(0, rest.length - trail.length).replace(/(?<!\\)\*/g, '\\*')
  return lead + core + trail
}

export function quote(value, escapeBackslash = true) {
  const v = escapeBackslash ? value.replace(/\\/g, '\\\\') : value
  return '"' + v.replace(/"/g, '\\"') + '"'
}

function renderValue(value, modifier) {
  if (modifier === 'regexp') return quote(value)
  const v = normalizeWildcards(value)
  return SIMPLE.test(v) && !['AND', 'OR', 'NOT', 'IS'].includes(v.toUpperCase()) ? v : quote(v, false)
}

export function render(call, warnings = []) {
  const conditions = Array.isArray(call?.conditions) ? call.conditions : []
  if (conditions.length === 0) warnings.push('Keine Bedingungen erkannt.')
  let depth = 0
  const out = []
  conditions.forEach((condition) => {
    const c = condition && typeof condition === 'object' ? condition : {}
    let field = String(c.field ?? '').toLowerCase()
    if (!FIELDS.includes(field)) {
      warnings.push(`Unbekanntes oder fehlendes Feld "${c.field}"; verwende lemma.`)
      field = 'lemma'
    }
    let op = c.op ?? '=='
    if (!OPERATORS.includes(op)) {
      warnings.push(`Unbekannter Operator "${op}"; verwende =.`)
      op = '=='
    }
    let modifier = c.modifier || ''
    if (modifier && !MODIFIERS.includes(modifier)) {
      warnings.push(`Unbekannter Modifier "${modifier}"; lasse ihn weg.`)
      modifier = ''
    }
    const value = String(c.value ?? '').trim()
    if (!value) {
      warnings.push(`Bedingung für ${field} ohne Wert übersprungen.`)
      return
    }
    if (modifier === 'regexp') {
      try {
        new RegExp(value)
      } catch {
        warnings.push(`Ungültiger regulärer Ausdruck "${value}"; behandle den Wert als Text.`)
        modifier = ''
      }
    }
    const requestedOpen = Math.max(0, Math.trunc(Number(c.open_paren) || 0))
    const requestedClose = Math.max(0, Math.trunc(Number(c.close_paren) || 0))
    const open = Math.min(requestedOpen, conditions.length)
    if (open !== requestedOpen) warnings.push('Zu viele öffnende Klammern; Anzahl wurde begrenzt.')
    let connector = String(c.connector ?? 'AND').toUpperCase()
    if (!CONNECTORS.includes(connector)) {
      warnings.push(`Unbekannter Verknüpfer "${c.connector}"; verwende AND.`)
      connector = 'AND'
    }

    const parts = []
    if (out.length > 0) parts.push(c.negate === true ? 'NOT' : connector)
    else if (c.negate === true) parts.push('NOT')
    depth += open
    const close = Math.min(requestedClose, depth)
    if (close !== requestedClose) warnings.push('Zu viele schließende Klammern; Anzahl wurde angepasst.')
    parts.push('('.repeat(open) + `${field} ${op}${modifier ? ' /' + modifier : ''} ${renderValue(value, modifier)}` + ')'.repeat(close))
    depth -= close
    out.push(parts.join(' '))
  })
  if (depth > 0) {
    out[out.length - 1] += ')'.repeat(depth)
    warnings.push('Fehlende schließende Klammern wurden ergänzt.')
  }
  if (out.length === 0) {
    warnings.push('Keine verwendbare Bedingung; es wird kein Query ausgegeben.')
    return ''
  }
  return out.join(' ')
}
