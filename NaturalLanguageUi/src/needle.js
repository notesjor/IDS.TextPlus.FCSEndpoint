let worker
let nextRequestId = 0
const pendingRequests = new Map()

function getWorker() {
  if (worker) return worker

  worker = new Worker(new URL('./needle.worker.js', import.meta.url), { type: 'module' })
  worker.addEventListener('message', ({ data }) => {
    const resolve = pendingRequests.get(data.id)
    if (!resolve) return
    pendingRequests.delete(data.id)
    resolve(data.result)
  })
  worker.addEventListener('error', (event) => {
    const message = event.message || 'Unbekannter Fehler im Needle-Worker.'
    for (const resolve of pendingRequests.values()) {
      resolve({ query: '', output: '', args: null, warnings: [message] })
    }
    pendingRequests.clear()
    worker.terminate()
    worker = undefined
  })
  return worker
}

export function translate(text) {
  return new Promise((resolve) => {
    const id = nextRequestId++
    pendingRequests.set(id, resolve)
    getWorker().postMessage({ id, text })
  })
}
