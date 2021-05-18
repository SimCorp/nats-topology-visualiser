<template>
<div id="app">
  <h1 id="title">Topology Visualizer</h1>
  <b-spinner id="load" label="Loading..."></b-spinner>
  <Graph
    ref="graph"
    @node-click="onNodeClick"
    :key="renderKey"
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
  <b-button class="refresh" id="rb" @click="refreshGraph" variant="info">&#8635;</b-button>
  <b-button class="refresh" id="rs" variant="info" disabled>
    <b-spinner small></b-spinner>
    <span class="sr-only">Loading...</span>
  </b-button>
  <Searchbar id="search" v-on:input="onSearchInput" @button-click="onSearchReset"/>
  <InfoPanel ref="panel"></InfoPanel>
  <Statusbar ref="status" :shouldDisplay="this.showStatus" :timeOfRequest="this.timeOfRequest"></Statusbar>
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
    showStatus: boolean;
    renderKey: number;
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
      showStatus: false,
      renderKey: 0
    }
  },
  mounted: async function() {
      this.displaySpinner(true)
      this.dataLoaded = await this.getData()
      this.displaySpinner(false)
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
    },
    displaySpinner (b: boolean) { // Used when reloading the page (F5)
      const spinner = document.getElementById("load")
      const refresh = document.getElementById("rb")
      if (b) {
        spinner.style.display = "block"
        refresh.style.display = "none"
        this.showStatus = false
      } else {
        spinner.style.display = "none"
        refresh.style.display = "block"
        this.showStatus = true
      }
    },
    displayRefreshLoad (b: boolean) { // Used when pushing the Refresh button
      const spin = document.getElementById("rs")
      const button = document.getElementById("rb")
      if (b) {
        spin.style.display = "block"
        button.style.display = "none"
      } else {
        spin.style.display = "none"
        button.style.display = "block"
      }
    },
    async refreshGraph () {
      this.displayRefreshLoad(true)
      this.dataLoaded = await this.getData()
      this.displayRefreshLoad(false)
      this.renderKey += 1
    }
  }
}

</script>

<style scoped>
#title {
  text-align: center;
  margin-top: 15px;
}

#load {
  position: fixed;
  left: 48%;
  bottom: 48%;
  width: 3rem;
  height: 3rem;
}

.refresh {
  position: fixed;
  bottom: 70px;
  left: 20px;
}

#rs {
  display: none;
}
</style>
