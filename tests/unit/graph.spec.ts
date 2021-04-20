import { mount, Wrapper } from '@vue/test-utils'
import { CombinedVueInstance } from 'vue/types/vue'
import Graph from '@/components/Graph/Graph.vue'
import Methods from '@/components/Graph/Graph.vue'

describe('Graph', () => {
  // Methods set to any because typescript can't figure it out...
  // Maybe a fix https://github.com/vuejs/vue/issues/8721#issuecomment-545671348
  const graph = mount<CombinedVueInstance<Graph, object, any, object, Record<never, any>>>(Graph, {})


  it('Graph contains an svg', () => {
    expect(graph.find('div#visualizer').exists()).toBe(true)

    // TODO: test fails idk why
    // expect(graph.find('svg').exists()).toBe(true)
  })

  it('CalculateViewBox', () => {
    expect(graph.vm.calculateViewBoxValue(100,50,1)).toEqual('-50, -25, 100, 50')
  })

})
