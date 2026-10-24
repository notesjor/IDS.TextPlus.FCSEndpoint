import { LEXCQL_TOOL, render } from './lexcql.js'

const MODEL_URL = 'https://huggingface.co/Cactus-Compute/needle3/resolve/main/needle3.cact'
const TOOL_GUIDANCE =
  'Translate the request into the lexcql_query tool call. Return a conditions array. Every condition MUST include a non-empty field and value. ' +
  'Choose field exactly: lemma for word form or beginning/ending patterns; pos for part of speech (NOUN, VERB, ADJ); ' +
  'definition for meaning; gender for grammatical gender. Use one condition per constraint. ' +
  'Example: verbs beginning "ver" but not ending "en" require lemma == ver*, pos == VERB, and NOT lemma == *en.'
let enginePromise

async function init() {
  const { default: initWasm, NeedleV3Wasm, extract_tool_call_v3 } = await import('needle-rs')
  await initWasm()

  const response = await fetch(MODEL_URL)
  if (!response.ok) throw new Error(`Modell-Download fehlgeschlagen (HTTP ${response.status}).`)
  const model = NeedleV3Wasm.load(new Uint8Array(await response.arrayBuffer()))
  if (!model) throw new Error('Die geladene Datei ist kein gültiges Needle-3-Modell.')
  return { model, extractToolCall: extract_tool_call_v3 }
}

function parseCallArguments(result) {
  if (!result) return null
  try {
    const calls = JSON.parse(result)
    const call = Array.isArray(calls) && calls.find((item) => item.name === LEXCQL_TOOL.name)
    if (!call) return null
    return typeof call.arguments === 'string' ? JSON.parse(call.arguments) : call.arguments
  } catch {
    return null
  }
}

function hasUsableCondition(args) {
  return Array.isArray(args?.conditions) && args.conditions.some((condition) => String(condition?.value ?? '').trim())
}

async function translate(text) {
  const warnings = []
  let output = ''
  try {
    enginePromise ??= init().catch((error) => {
      enginePromise = undefined
      throw new Error('Needle-WASM konnte nicht geladen werden: ' + error.message)
    })
    const { model, extractToolCall } = await enginePromise
    output = model.run(`${TOOL_GUIDANCE}\nRequest: ${text}`, JSON.stringify([LEXCQL_TOOL]))
    const args = parseCallArguments(extractToolCall(output))
    if (!hasUsableCondition(args)) {
      warnings.push('Needle hat keine verwertbaren Bedingungen geliefert. Die vollständige Modellausgabe steht im Debug-Bereich.')
      return { query: '', output, args, warnings }
    }
    return { query: render(args, warnings), output, args, warnings }
  } catch (error) {
    warnings.push(error.message)
    return { query: '', output, args: null, warnings }
  }
}

self.addEventListener('message', async ({ data }) => {
  const result = await translate(data.text)
  self.postMessage({ id: data.id, result })
})