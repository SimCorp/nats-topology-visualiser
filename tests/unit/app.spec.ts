import { shallowMount } from '@vue/test-utils'
import App from '@/App.vue'

describe('App', () => {
  const app = shallowMount(App)

  it('App has a div with id="app"', () => {
    expect(app.find('div#app').exists()).toBe(true)
  })
})
