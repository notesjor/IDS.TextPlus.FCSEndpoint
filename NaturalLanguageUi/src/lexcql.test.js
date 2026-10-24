import test from 'node:test'
import assert from 'node:assert/strict'
import { LEXCQL_SCHEMA, render } from './lexcql.js'

test('tool schema requires each condition to have a field and value', () => {
  assert.deepEqual(LEXCQL_SCHEMA.properties.conditions.items.required, ['field', 'value'])
})

test('wildcard + AND', () => {
  assert.equal(
    render({ conditions: [
      { field: 'lemma', value: '*in' }, { field: 'pos', value: 'NOUN' },
      { field: 'definition', value: 'Person' }, { field: 'gender', value: 'Fem' }] }),
    'lemma == *in AND pos == NOUN AND definition == Person AND gender == Fem')
})
test('regexp, NOT, OR, parens, IS', () => {
  assert.equal(
    render({ conditions: [
      { field: 'lemma', modifier: 'regexp', value: '^a.*in$', open_paren: 1 },
      { field: 'lemma', value: 'Berg*', connector: 'OR', close_paren: 1 },
      { field: 'pos', op: 'IS', value: 'NOUN', negate: true }] }),
    '(lemma == /regexp "^a.*in$" OR lemma == Berg*) NOT pos IS NOUN')
})
test('inner wildcard escaped, unbalanced parens corrected', () => {
  assert.equal(render({ conditions: [{ field: 'lemma', value: 'a*b' }] }), 'lemma == a\\*b')
  const warnings = []
  assert.equal(render({ conditions: [{ field: 'lemma', value: 'a', open_paren: 1 }] }, warnings), '(lemma == a)')
  assert.ok(warnings.length > 0)
})

test('missing field defaults to lemma and invalid parts are repaired', () => {
  const warnings = []
  assert.equal(
    render({ conditions: [
      { value: 'Berg*', op: 'CONTAINS', modifier: 'unknown', open_paren: 1 },
      { field: 'pos', value: 'NOUN', connector: 'XOR' },
    ] }, warnings),
    '(lemma == Berg* AND pos == NOUN)')
  assert.ok(warnings.length >= 4)
})

test('empty or unusable conditions do not invent a fallback query', () => {
  const warnings = []
  assert.equal(render({ conditions: [{ field: 'pos', value: '  ' }] }, warnings), '')
  assert.ok(warnings.length > 0)
})

test('verb prefix, part of speech, and negated suffix render as requested', () => {
  assert.equal(
    render({ conditions: [
      { field: 'lemma', op: '==', value: 'ver*' },
      { field: 'pos', op: '==', value: 'VERB' },
      { field: 'lemma', op: '==', value: '*en', negate: true },
    ] }),
    'lemma == ver* AND pos == VERB NOT lemma == *en')
})
