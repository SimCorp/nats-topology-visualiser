// Example unit test
describe('Predicate Logic', () => {
  // Shout out Alessandro
  const imp = (A: boolean, B: boolean) => !A || B
  it('Implication truth table', () => {
    expect(imp(true, true)).toBe(true)
    expect(imp(false, true)).toBe(true)
    expect(imp(false, false)).toBe(true)
    expect(imp(true, false)).toBe(false)
  })
})
