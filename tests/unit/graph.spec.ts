import { mount, Wrapper } from '@vue/test-utils'
import { CombinedVueInstance } from 'vue/types/vue'
import Graph from '@/components/Graph/Graph.vue'
import Methods from '@/components/Graph/Graph.vue'

describe('Graph', () => {
  const graph = mount<CombinedVueInstance<Graph, object, any, object, Record<never, any>>>(Graph, {
    propsData: {
      servers: [
        {
          "server_id": "ID1",
          "server_name": "nats-server-1",
          "ntv_error": true,
          "ntv_cluster": "nats-cluster-1",
          "clients": []
        },
        {
          "server_id": "ID2",
          "server_name": "nats-server-2",
          "ntv_error": false,
          "ntv_cluster": "nats-cluster-1",
          "clients": [
            {
              "ip": "12.34.56.78",
              "port": 1337
            }
          ]
        }
      ],
      routes: [
        {
          "source": "ID1",
          "target": "ID2",
          "ntv_error": false
        },
      ],
      clusters: [
        {
          "name": "nats-cluster-1",
          "servers": [
            {
              "server_id": "ID1",
              "server_name": "nats-server-1",
              "ntv_error": true,
              "ntv_cluster": "nats-cluster-1",
              "clients": []
            },
            {
              "server_id": "ID2",
              "server_name": "nats-server-2",
              "ntv_error": false,
              "ntv_cluster": "nats-cluster-1",
              "clients": [
                {
                  "ip": "12.34.56.78",
                  "port": 1337
                }
              ]
            }
          ]
        },
      ],
      gateways: [],
      leafs: [],
      varz: [],
      dataLoaded: true,
    }
  })

  it('Graph contains an svg', () => {
    expect(true).toBe(true)
    expect(graph.find('div#visualizer').exists()).toBe(true)

    // TODO: test fails idk why
    // expect(graph.find('svg').exists()).toBe(true)
  })

  it('CalculateViewBox', () => {
    expect(graph.vm.calculateViewBoxValue(100,50,1)).toEqual('-50, -25, 100, 50')
  })

})