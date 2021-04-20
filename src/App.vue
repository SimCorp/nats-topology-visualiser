<template>
<div id="app">
  <h1>Topology Visualizer</h1>
  <Graph
    v-if='dataLoaded'
    :servers='this.servers'
    :routes='this.routes'
    :clusters='this.clusters'
    :gatewayLinks='this.gatewayLinks'
    :dataLoaded='this.dataLoaded'
  >
  </Graph>
  <Statusbar/>
</div>
</template>

<script lang="ts">
import axios from 'axios'

import Graph from '@/components/Graph/Graph.vue'
import ServerDatum from './components/Graph/ServerDatum'
import RouteDatum from './components/Graph/RouteDatum'
import ClusterDatum from './components/Graph/ClusterDatum'
import GatewayDatum from './components/Graph/GatewayDatum'
import Statusbar from '@/components/Statusbar.vue'

export default {
  name: 'App',
  components: { Graph, Statusbar },
  data (): { 
    servers: ServerDatum[]; 
    routes: RouteDatum[];
    clusters: ClusterDatum[];
    gatewayLinks: GatewayDatum[];
    dataLoaded: boolean;
  } {
    return {
      servers: [],
      routes: [],
      clusters: [],
      gatewayLinks: [],
      dataLoaded: false,
    }
  },
  created: async function(){
      this.dataLoaded = await this.getData()
      console.log('app dataLoaded', this.$data)
  },
  methods: {
    async getData (): Promise<boolean> {
      const host = 'https://localhost:5001'

      this.servers = (await axios.get(`${host}/nodes`)).data
      this.routes = (await axios.get(`${host}/links`)).data
      this.clusters =( await axios.get(`${host}/clusters`)).data
      this.gatewayLinks = (await axios.get(`${host}/gatewayLinks`)).data

      return true
    },
  }
}

</script>

<style scoped>
#title {
  text-align: center;
  margin-top: 15px;
}
</style>
