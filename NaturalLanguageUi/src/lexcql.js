// Renders structured conditions (Needle tool call) into a LexCQL string
// that is accepted by LexCqlTokenizer / LexCqlParser.

export const FIELDS = ['lemma', 'pos', 'definition', 'gender']
export const OPERATORS = ['=', '==', '!=', 'IS']
export const MODIFIERS = ['ignoreCase', 'respectCase', 'regexp']
export const CONNECTORS = ['AND', 'OR']

/** Tool definition handed to Needle. */
export const LEXCQL_TOOL = {
  name: 'lexcql_query',
  description:
    'Build a lexical query. Each condition compares a field with a value. ' +
    'Fields: lemma (word form; * wildcard only at start and/or end, e.g. *in, Berg*), pos (UPOS: NOUN, VERB, ADJ, ...), ' +
    'definition, gender (Masc, Fem, Neut). Operators: =, ==, !=, IS. ' +
    'Modifier "regexp" treats value as regular expression. ' +
    'connector (AND/OR) joins a condition with the previous one, negate adds NOT, ' +
    'open_paren/close_paren group conditions.',
  parameters: {
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
  },
}

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

export function render(call) {
  const conditions = call?.conditions
  if (!Array.isArray(conditions) || conditions.length === 0) throw new Error('Keine Bedingungen erkannt.')
  let depth = 0
  const out = []
  conditions.forEach((c, i) => {
    const field = String(c.field ?? '').toLowerCase()
    const op = c.op ?? '='
    const value = String(c.value ?? '').trim()
    const modifier = c.modifier || ''
    if (!FIELDS.includes(field)) throw new Error(`Unbekanntes Feld: ${c.field}`)
    if (!OPERATORS.includes(op)) throw new Error(`Unbekannter Operator: ${op}`)
    if (modifier && !MODIFIERS.includes(modifier)) throw new Error(`Unbekannter Modifier: ${modifier}`)
    if (!value) throw new Error('Leerer Wert.')
    if (modifier === 'regexp') {
      try { new RegExp(value) } catch { throw new Error(`Ungültiger regulärer Ausdruck: ${value}`) }
    }
    const open = Math.max(0, c.open_paren | 0)
    const close = Math.max(0, c.close_paren | 0)
    const connector = String(c.connector ?? 'AND').toUpperCase()
    if (!CONNECTORS.includes(connector)) throw new Error(`Unbekannter Verknüpfer: ${c.connector}`)

    const parts = []
    if (i > 0) parts.push(connector)
    if (c.negate) parts.push('NOT')
    parts.push('('.repeat(open) + `${field} ${op}${modifier ? ' /' + modifier : ''} ${renderValue(value, modifier)}` + ')'.repeat(close))
    depth += open - close
    if (depth < 0) throw new Error('Unausgeglichene Klammern.')
    out.push(parts.join(' '))
  })
  if (depth !== 0) throw new Error('Unausgeglichene Klammern.')
  return out.join(' ')
}
