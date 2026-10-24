<script setup>
import { computed, ref } from 'vue'
import { translate, DEFAULT_URL, MODEL } from './ollama.js'

const input = ref('')
const ollamaUrl = ref(localStorage.getItem('ollamaUrl') || DEFAULT_URL)
const query = ref('')
const modelOutput = ref('')
const modelArgs = ref(null)
const warnings = ref([])
const loading = ref(false)
const copied = ref(false)
const copiedDebug = ref('')
const formattedModelOutput = computed(() => modelOutput.value
  .replace(/<think>/g, '<think>\n')
  .replace(/<\/think>/g, '\n</think>')
  .replace(/<tool_call>/g, '\n<tool_call>\n')
  .replace(/<\/tool_call>/g, '\n</tool_call>'))

const examples = [
  'Ich suche weibliche Nomen, die auf -in enden und sich auf eine Person beziehen',
  'Verben, die mit "ver" beginnen, aber nicht auf "en" enden',
  'Nomen oder Adjektive, die dem regulären Ausdruck ^[A-Z]a.*t$ entsprechen',
]

async function run() {
  if (!input.value.trim() || loading.value) return
  loading.value = true
  warnings.value = []
  query.value = ''
  modelOutput.value = ''
  modelArgs.value = null
  try {
    localStorage.setItem('ollamaUrl', ollamaUrl.value)
    const r = await translate(input.value, ollamaUrl.value)
    query.value = r.query
    modelOutput.value = r.output
    modelArgs.value = r.args
    warnings.value = r.warnings ?? []
  } catch (e) {
    warnings.value = [e.message]
  } finally {
    loading.value = false
  }
}

async function copy() {
  await navigator.clipboard.writeText(query.value)
  copied.value = true
  setTimeout(() => (copied.value = false), 1500)
}

async function copyDebug(value, kind) {
  await navigator.clipboard.writeText(value)
  copiedDebug.value = kind
  setTimeout(() => (copiedDebug.value = ''), 1500)
}
</script>

<template>
  <v-app>
    <v-main class="d-flex align-center justify-center bg-gradient">
      <v-container style="max-width: 760px">
        <div class="text-center mb-8">
          <v-icon icon="mdi-text-search-variant" size="56" color="primary" />
          <h1 class="text-h4 font-weight-bold mt-2">Abfrage in natürlicher Sprache</h1>
          <p class="text-medium-emphasis">Wird mit Ollama ({{ MODEL }}) in LexCQL übersetzt.</p>
        </div>

        <v-card rounded="xl" elevation="8" class="pa-6">
          <v-text-field v-model="ollamaUrl" label="Ollama-URL" density="compact" variant="outlined" prepend-inner-icon="mdi-server" class="mb-4" hide-details />
          <v-textarea
            v-model="input"
            label="Deine Abfrage"
            rows="3"
            auto-grow
            variant="outlined"
            autofocus
            hide-details
            @keydown.ctrl.enter.prevent="run"
          />
          <div class="d-flex flex-wrap ga-2 my-4">
            <v-chip v-for="e in examples" :key="e" size="small" @click="input = e">{{ e }}</v-chip>
          </div>
          <v-btn block size="large" color="primary" :loading="loading" :disabled="!input.trim()" @click="run">
            <v-icon start icon="mdi-auto-fix" /> {{ loading ? 'Ollama arbeitet …' : 'In LexCQL übersetzen' }}
          </v-btn>

          <v-alert v-if="warnings.length" type="warning" variant="tonal" class="mt-4">
            <div v-for="(warning, index) in warnings" :key="index">{{ warning }}</div>
          </v-alert>

          <div v-if="query" class="mt-6">
            <div class="d-flex align-center mb-1">
              <span class="text-overline">LexCQL</span>
              <v-spacer />
              <v-btn size="small" variant="text" :prepend-icon="copied ? 'mdi-check' : 'mdi-content-copy'" @click="copy">
                {{ copied ? 'Kopiert' : 'Kopieren' }}
              </v-btn>
            </div>
            <v-sheet rounded="lg" color="surface-variant" class="pa-4 text-body-1" style="font-family: monospace; word-break: break-all">
              {{ query }}
            </v-sheet>
          </div>
          <v-expansion-panels v-if="modelOutput || modelArgs" variant="accordion" class="mt-3">
            <v-expansion-panel title="Debug-Ausgabe">
              <v-expansion-panel-text>
                <div class="d-flex align-center justify-space-between mb-2">
                  <span class="text-subtitle-2">Originalausgabe</span>
                  <v-btn
                    size="small"
                    variant="text"
                    :prepend-icon="copiedDebug === 'output' ? 'mdi-check' : 'mdi-content-copy'"
                    :disabled="!modelOutput"
                    @click="copyDebug(modelOutput, 'output')"
                  >
                    {{ copiedDebug === 'output' ? 'Kopiert' : 'Ausgabe kopieren' }}
                  </v-btn>
                </div>
                <pre class="debug-output">{{ formattedModelOutput || 'Keine Modellausgabe vorhanden.' }}</pre>
                <template v-if="modelArgs">
                  <div class="d-flex align-center justify-space-between mt-4 mb-2">
                    <span class="text-subtitle-2">Extrahierte Tool-Argumente</span>
                    <v-btn
                      size="small"
                      variant="text"
                      :prepend-icon="copiedDebug === 'args' ? 'mdi-check' : 'mdi-content-copy'"
                      @click="copyDebug(JSON.stringify(modelArgs, null, 2), 'args')"
                    >
                      {{ copiedDebug === 'args' ? 'Kopiert' : 'JSON kopieren' }}
                    </v-btn>
                  </div>
                  <pre class="debug-output">{{ JSON.stringify(modelArgs, null, 2) }}</pre>
                </template>
              </v-expansion-panel-text>
            </v-expansion-panel>
          </v-expansion-panels>
        </v-card>
      </v-container>
    </v-main>
  </v-app>
</template>

<style>
.bg-gradient {
  min-height: 100vh;
  background: radial-gradient(circle at 20% 10%, #1e2a4a 0%, #12121a 60%);
}
.debug-output {
  overflow-x: auto;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
  font-family: monospace;
}
</style>
