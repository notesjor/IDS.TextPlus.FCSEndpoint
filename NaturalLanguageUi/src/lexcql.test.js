import test from 'node:test'
import assert from 'node:assert/strict'
import { render } from './lexcql.js'

test('wildcard + AND', () => {
  assert.equal(
    render({ conditions: [
      { field: 'lemma', value: '*in' }, { field: 'pos', value: 'NOUN' },
      { field: 'definition', value: 'Person' }, { field: 'gender', value: 'Fem' }] }),
    'lemma = *in AND pos = NOUN AND definition = Person AND gender = Fem')
})
test('regexp, NOT, OR, parens, IS', () => {
  assert.equal(
    render({ conditions: [
      { field: 'lemma', modifier: 'regexp', value: '^a.*in$', open_paren: 1 },
      { field: 'lemma', value: 'Berg*', connector: 'OR', close_paren: 1 },
      { field: 'pos', op: 'IS', value: 'NOUN', negate: true }] }),
    '(lemma = /regexp "^a.*in$" OR lemma = Berg*) AND NOT pos IS NOUN')
})
test('inner wildcard escaped, bad parens rejected', () => {
  assert.equal(render({ conditions: [{ field: 'lemma', value: 'a*b' }] }), 'lemma = a\\*b')
  assert.throws(() => render({ conditions: [{ field: 'lemma', value: 'a', open_paren: 1 }] }))
})
