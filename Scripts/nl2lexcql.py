#!/usr/bin/env python3
"""Translate a natural-language query into LexCQL using a Needle model (Cactus Compute).

Needle is used as a function-calling model: it is given one tool, `lexcql_query`,
and returns structured conditions. Those are validated and rendered to a LexCQL
string, which can be consumed by LexCqlTokenizer/LexCqlParser.

Backend: any OpenAI-compatible chat endpoint serving Needle (e.g. a local Cactus server).
  NEEDLE_URL   default http://localhost:8080/v1/chat/completions
  NEEDLE_MODEL default needle-3
Usage: python nl2lexcql.py "Ich suche weibliche Nomen, die auf -in enden und sich auf eine Person beziehen"
       python nl2lexcql.py --offline-json '{"conditions":[...]}'   (skip model call)
"""
import argparse, json, os, re, sys, urllib.request

FIELDS = {"lemma", "pos", "definition", "gender"}
OPS = {"=", "==", "!="}
LOGIC = {"AND", "OR"}

TOOL = {
    "type": "function",
    "function": {
        "name": "lexcql_query",
        "description": "Build a lexical query from conditions on dictionary entries. "
                       "lemma supports * wildcards (e.g. *in = ends with 'in'). "
                       "pos uses UPOS tags (NOUN, VERB, ADJ, ...). gender: Masc, Fem, Neut.",
        "parameters": {
            "type": "object",
            "properties": {
                "logic": {"type": "string", "enum": ["AND", "OR"],
                          "description": "How conditions are combined"},
                "conditions": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "field": {"type": "string", "enum": sorted(FIELDS)},
                            "op": {"type": "string", "enum": ["=", "==", "!="]},
                            "value": {"type": "string"},
                        },
                        "required": ["field", "value"],
                    },
                },
            },
            "required": ["conditions"],
        },
    },
}


def call_needle(text: str) -> dict:
    url = os.environ.get("NEEDLE_URL", "http://localhost:8080/v1/chat/completions")
    body = {"model": os.environ.get("NEEDLE_MODEL", "needle-3"),
            "messages": [{"role": "user", "content": text}],
            "tools": [TOOL], "temperature": 0}
    req = urllib.request.Request(url, json.dumps(body).encode(), {"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as r:
        msg = json.load(r)["choices"][0]["message"]
    calls = msg.get("tool_calls") or []
    if not calls:
        raise ValueError("Model returned no tool call: " + str(msg.get("content")))
    args = calls[0]["function"]["arguments"]
    return json.loads(args) if isinstance(args, str) else args


def quote(v: str) -> str:
    if re.fullmatch(r"[^\s\"()=!/]+", v):
        return v
    return '"' + v.replace("\\", "\\\\").replace('"', '\\"') + '"'


def render(call: dict) -> str:
    logic = str(call.get("logic", "AND")).upper()
    if logic not in LOGIC:
        raise ValueError(f"Invalid logic: {logic}")
    parts = []
    for c in call.get("conditions", []):
        field, op, value = str(c.get("field", "")).lower(), c.get("op", "="), str(c.get("value", "")).strip()
        if field not in FIELDS or op not in OPS or not value:
            raise ValueError(f"Invalid condition: {c}")
        parts.append(f"{field} {op} {quote(value)}")
    if not parts:
        raise ValueError("No conditions")
    return f" {logic} ".join(parts)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("query", nargs="?")
    ap.add_argument("--offline-json", help="tool-call arguments as JSON (skips model call)")
    a = ap.parse_args()
    if a.offline_json:
        call = json.loads(a.offline_json)
    elif a.query:
        call = call_needle(a.query)
    else:
        ap.error("query or --offline-json required")
    print(render(call))


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        sys.exit(f"error: {e}")
