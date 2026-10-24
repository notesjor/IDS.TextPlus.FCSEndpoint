# LexCQL – natürlichsprachliche Abfrage (Vue + Vuetify + Vite)

Eingabe → Button → LexCQL-Abfrage, übersetzt per Ollama. Kein Request gegen den Suchdienst.

```
bun install   # oder npm install
bun run dev   # oder npm run dev
bun run test  # Tests für den LexCQL-Renderer
```

## Ollama
Die Abfrage wird per `POST <URL>/api/chat` (Structured Output, Modell `qwen2.5-coder:0.5b`) an Ollama geschickt.
URL: Eingabefeld in der UI (wird im Browser gespeichert) oder Standardwert über `VITE_OLLAMA_URL` (Default `http://localhost:11434`).
Voraussetzung: `ollama pull qwen2.5-coder:0.5b`; bei anderer Herkunft ggf. `OLLAMA_ORIGINS` setzen (CORS), z. B. `OLLAMA_ORIGINS=http://localhost:5173`.

## Unterstützte Syntax (`src/lexcql.js`)
- Felder: `lemma`, `pos`, `definition`, `gender`
- Operatoren: `=`, `==`, `!=`, `IS`; Verknüpfung `AND`, `OR`, `NOT`; Klammern `( )`
- Modifier: `/ignoreCase`, `/respectCase`, `/regexp` (Wert wird dann als RegEx validiert und gequotet)
- Wildcard `*` nur am Wortanfang/-ende (`*in`, `Berg*`); innere `*` werden als `\*` escaped
