<template>
<div id="app">
  <h1 id="title">Topology Visualizer</h1>
  <Graph
    ref="graph"
    @node-click="onNodeClick"
    v-if='dataLoaded'
    :servers='this.servers'
    :routes='this.routes'
    :clusters='this.clusters'
    :gateways='this.gateways'
    :leafs='this.leafs'
    :varz='this.varz'
    :dataLoaded='this.dataLoaded'
  >
  </Graph>
  <Searchbar id="search" v-on:input="onSearchInput" @button-click="onSearchReset"/>
  <InfoPanel ref="panel"></InfoPanel>
  <Statusbar :timeOfRequest="this.timeOfRequest" ></Statusbar>
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
import Searchbar from "@/components/Searchbar.vue"
import LeafDatum from './components/Graph/LeafDatum'
import Varz from './components/Graph/Varz'

export default {
  name: 'App',
  components: {
    Graph,
    Statusbar,
    Searchbar,
    InfoPanel
  },
  data (): {
    servers: ServerDatum[];
    routes: RouteDatum[];
    clusters: ClusterDatum[];
    gateways: GatewayDatum[];
    leafs: LeafDatum[]; // TODO add LeafDatum?
    varz: Varz[];
    timeOfRequest: Date;
    dataLoaded: boolean;
    isPanelOpen: boolean;
  } {
    return {
      servers: [],
      routes: [],
      clusters: [],
      gateways: [],
      leafs: [],
      varz: [],
      timeOfRequest: undefined,
      dataLoaded: false,
      isPanelOpen: false,
    }
  },
  created: async function(){
      this.dataLoaded = await this.getData()
      console.log('app dataLoaded', this.$data)
  },
  methods: {
    async getData (): Promise<boolean> {
      const host = 'https://localhost:5001'
      // TODO add type safety
      const data = (await axios.get(`${host}/updateEverything`)).data

      // TODO why no type errors?
      this.servers = data.processedServers
      this.routes = data.links
      this.clusters = data.processedClusters
      this.gateways = data.gatewayLinks
      this.leafs = data.leafLinks
      this.varz = data.varz
      this.timeOfRequest = data.timeOfRequest
      return true
    },
    onNodeClick ({nodeData, id}) {
      this.$refs.panel.onNodeClick({nodeData, id})
    },
    onSearchInput (text: string) {
      this.$refs.graph.searchFilter(text)
    },
    onSearchReset () {
      this.$refs.graph.searchReset()
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
