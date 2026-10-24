import { LEXCQL_TOOL, render } from './lexcql.js'

// The Needle 3 WASM runtime and model are loaded at runtime from /needle/ (see README),
// so no unverified npm dependency is baked in. Adjust this file if your runtime's API differs.
// Expected API: loadEngine({ wasmPath, modelPath }) and new NeedleAgent(engine, tools).run(text)
//   -> { function_calls: [{ name, arguments }] }
const BASE = import.meta.env.BASE_URL + 'needle/'
let agentPromise

async function init() {
  const mod = await import(/* @vite-ignore */ BASE + 'needle.js')
  const engine = await mod.loadEngine({ wasmPath: BASE + 'needle.wasm', modelPath: BASE + 'needle3.cact' })
  return new mod.NeedleAgent(engine, [LEXCQL_TOOL])
}

export async function translate(text) {
  agentPromise ??= init().catch((e) => {
    agentPromise = undefined
    throw new Error('Needle-WASM konnte nicht geladen werden (siehe README): ' + e.message)
  })
  const agent = await agentPromise
  const result = await agent.run(text)
  const call = (result.function_calls ?? []).find((c) => c.name === LEXCQL_TOOL.name)
  if (!call) throw new Error('Das Modell hat keine Abfrage erzeugt.')
  const args = typeof call.arguments === 'string' ? JSON.parse(call.arguments) : call.arguments
  return { query: render(args), raw: args }
}
