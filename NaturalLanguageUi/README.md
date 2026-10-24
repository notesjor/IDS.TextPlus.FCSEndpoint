# LexCQL – natürlichsprachliche Abfrage (Vue + Vuetify + Vite)

Eingabe → Button → LexCQL-Abfrage, übersetzt lokal im Browser mit Needle 3 (WASM). Kein Request gegen den Suchdienst.

```
bun install   # oder npm install
bun run dev   # oder npm run dev
bun run test  # Tests für den LexCQL-Renderer
```

## Needle 3 einbinden
Die WASM-Runtime wird über `needle-rs` eingebunden. Beim ersten Übersetzen lädt der Browser das Needle-3-Modell (35,3 MB) von Hugging Face; dafür ist eine Internetverbindung nötig. Danach bleibt das Modell bis zum Neuladen der Seite im Speicher.

Das Modell steht unter Apache-2.0. Das Runtime-Paket ist MIT-lizenziert.

## Unterstützte Syntax (`src/lexcql.js`)
- Felder: `lemma`, `pos`, `definition`, `gender`
- Operatoren: `=`, `==`, `!=`, `IS`; Verknüpfung `AND`, `OR`, `NOT`; Klammern `( )`
- Modifier: `/ignoreCase`, `/respectCase`, `/regexp` (Wert wird dann als RegEx validiert und gequotet)
- Wildcard `*` nur am Wortanfang/-ende (`*in`, `Berg*`); innere `*` werden als `\*` escaped
