<script setup>
import { ref } from 'vue'
import { translate } from './needle.js'

const input = ref('')
const query = ref('')
const raw = ref(null)
const error = ref('')
const loading = ref(false)
const copied = ref(false)

const examples = [
  'Ich suche weibliche Nomen, die auf -in enden und sich auf eine Person beziehen',
  'Verben, die mit "ver" beginnen, aber nicht auf "en" enden',
  'Nomen oder Adjektive, die dem regulären Ausdruck ^[A-Z]a.*t$ entsprechen',
]

async function run() {
  if (!input.value.trim()) return
  loading.value = true
  error.value = ''
  query.value = ''
  raw.value = null
  try {
    const r = await translate(input.value)
    query.value = r.query
    raw.value = r.raw
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

async function copy() {
  await navigator.clipboard.writeText(query.value)
  copied.value = true
  setTimeout(() => (copied.value = false), 1500)
}
</script>

<template>
  <v-app>
    <v-main class="d-flex align-center justify-center bg-gradient">
      <v-container style="max-width: 760px">
        <div class="text-center mb-8">
          <v-icon icon="mdi-text-search-variant" size="56" color="primary" />
          <h1 class="text-h4 font-weight-bold mt-2">Abfrage in natürlicher Sprache</h1>
          <p class="text-medium-emphasis">Wird lokal im Browser mit Needle 3 (WASM) in LexCQL übersetzt.</p>
        </div>

        <v-card rounded="xl" elevation="8" class="pa-6">
          <v-textarea
            v-model="input"
            label="Deine Abfrage"
            rows="3"
            auto-grow
            variant="outlined"
            autofocus
            hide-details
            @keydown.ctrl.enter="run"
          />
          <div class="d-flex flex-wrap ga-2 my-4">
            <v-chip v-for="e in examples" :key="e" size="small" @click="input = e">{{ e }}</v-chip>
          </div>
          <v-btn block size="large" color="primary" :loading="loading" :disabled="!input.trim()" @click="run">
            <v-icon start icon="mdi-auto-fix" /> In LexCQL übersetzen
          </v-btn>

          <v-alert v-if="error" type="error" variant="tonal" class="mt-4">{{ error }}</v-alert>

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
            <v-expansion-panels variant="accordion" class="mt-3">
              <v-expansion-panel title="Rohausgabe des Modells">
                <v-expansion-panel-text><pre>{{ JSON.stringify(raw, null, 2) }}</pre></v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>
          </div>
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
</style>
