# LexCQL – natürlichsprachliche Abfrage (Vue + Vuetify + Vite)

Eingabe → Button → LexCQL-Abfrage, übersetzt lokal im Browser mit Needle 3 (WASM). Kein Request gegen den Suchdienst.

```
bun install   # oder npm install
bun run dev   # oder npm run dev
bun run test  # Tests für den LexCQL-Renderer
```

## Needle 3 einbinden
Lege Runtime und Modell nach `public/needle/` (nicht eingecheckt):
`needle.js` (exportiert `loadEngine`, `NeedleAgent`), `needle.wasm`, `needle3.cact`.
Weicht die API deiner Runtime ab, passe nur `src/needle.js` an.

## Unterstützte Syntax (`src/lexcql.js`)
- Felder: `lemma`, `pos`, `definition`, `gender`
- Operatoren: `=`, `==`, `!=`, `IS`; Verknüpfung `AND`, `OR`, `NOT`; Klammern `( )`
- Modifier: `/ignoreCase`, `/respectCase`, `/regexp` (Wert wird dann als RegEx validiert und gequotet)
- Wildcard `*` nur am Wortanfang/-ende (`*in`, `Berg*`); innere `*` werden als `\*` escaped
