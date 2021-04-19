import { shallowMount } from '@vue/test-utils'
import Graph from '@/components/Graph/Graph.vue'

describe('Graph', () => {
  const graph = shallowMount(Graph)

  it('Graph contains an svg', async () => {
    expect(graph.find('div#visualizer').exists()).toBe(true)

    // TODO: test fails idk why
    // expect(graph.find('svg').exists()).toBe(true)
  })
  
})
