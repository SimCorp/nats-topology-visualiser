<template>
<div id="app">
  <h1 id="title">Topology Visualizer</h1>
  <Graph
    @node-click="onNodeClick"
    v-if='dataLoaded'
    :servers='this.servers'
    :routes='this.routes'
    :clusters='this.clusters'
    :gatewayLinks='this.gatewayLinks'
    :dataLoaded='this.dataLoaded'
  >
  </Graph>
  <InfoPanel ref="panel"></InfoPanel>
  <Statusbar></Statusbar>
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
import InfoPanel from '@/components/InfoPanel.vue'


export default {
  name: 'App',
  components: {
    Graph,
    Statusbar,
    InfoPanel
  },
  data (): { 
    servers: ServerDatum[]; 
    routes: RouteDatum[];
    clusters: ClusterDatum[];
    gatewayLinks: GatewayDatum[];
    dataLoaded: boolean;
    isPanelOpen: boolean;
  } {
    return {
      servers: [],
      routes: [],
      clusters: [],
      gatewayLinks: [],
      dataLoaded: false,
      isPanelOpen: false
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
    onNodeClick () {
      this.$refs.panel.onNodeClick();
    }
  }
}

</script>

<style scoped>
#title {
  text-align: center;
  margin-top: 15px;
}

</style>
