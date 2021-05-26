<template>
<div id="app">
  <!-- Spinner on page reload -->
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
  ></Graph>
  <Searchbar id="search" v-on:input="onSearchInput" @button-click="onSearchReset" ref="search"></Searchbar>
  <div id="status">
    <Refresh ref="refresh" @button-click="refreshGraph"/>
    <Statusbar ref="status" :shouldDisplay="this.showStatus" :timeOfRequest="this.timeOfRequest"></Statusbar>
  </div>
  <InfoPanel ref="panel"></InfoPanel>
  <StructurePanel ref="structurepanel" id="structurePanel" v-on:structure-node-click="onStructureNodeClick" :treeNodes="this.treenodes" v-if='dataLoaded'></StructurePanel>
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
import StructurePanel from '@/components/StructurePanel.vue';

import Zoombar from '@/components/Zoombar.vue'
import LinkDatum from './components/Graph/LinkDatum'
import TreeList from '@/components/TreeList.vue'
import Searchbar from "@/components/Searchbar.vue"
import LeafDatum from './components/Graph/LeafDatum'
import Varz from './components/Graph/Varz'
import Refresh from "@/components/Refresh.vue"
import {id} from "postcss-selector-parser";

export default {
  name: 'App',
  components: {
    Refresh,
    Graph,
    Statusbar,
    Searchbar,
    InfoPanel,
    StructurePanel,
  },
  data (): {
    servers: ServerDatum[];
    routes: RouteDatum[];
    clusters: ClusterDatum[];
    gateways: GatewayDatum[];
    leafs: LeafDatum[]; // TODO add LeafDatum?
    varz: Varz[];
    treenodes: Array;
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
      treenodes: [],
      timeOfRequest: undefined,
      dataLoaded: false,
      isPanelOpen: false,
      showStatus: false,
      renderKey: 0
    }
  },
  mounted: async function() {
      this.showStatus = this.displayReloadSpinner(true)
      this.dataLoaded = await this.getData()
      this.showStatus = this.displayReloadSpinner(false)
  },
  methods: {
    async getData (): Promise<boolean> {
      const host = 
        (process.env.NODE_ENV === 'production')
        ? ''
        : 'https://localhost:5001'

      // TODO add type safety
      let mockData = true
      const data =
        (mockData)
        ? (await (await fetch('./mock-data-updateeverything.json')).json())
        : (await axios.get(`${host}/api/updateEverything`)).data

      // TODO why no type errors?
      this.servers = data.processedServers
      this.routes = data.links
      this.clusters = data.processedClusters
      this.gateways = data.gatewayLinks
      this.leafs = data.leafLinks
      this.varz = data.varz
      this.treenodes = data.treeNodes
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

    getServerWithId(server_id: string): Varz | string {
      for (const server of this.varz) {
        if (server.server_id === server_id) return server // TODO add varz type
      }
      return ""
    },

    onStructureNodeClick ({name, server_id}) {
      var nameStr = name.toString()
      var idStr = server_id.toString()
      this.$refs.graph.searchFilter(nameStr)
      this.$refs.search.changeText(nameStr)
      this.onNodeClick({nodeData: this.getServerWithId(idStr), id: idStr})
    },

    async refreshGraph () {
      this.$refs.refresh.displayRefreshSpinner(true)
      this.dataLoaded = await this.getData()
      this.$refs.refresh.displayRefreshSpinner(false)
      this.renderKey += 1 // Tells the Graph component to completely reload
    },
    displayReloadSpinner (b: boolean) { // Used when reloading the page (F5)
      const spinner = document.getElementById("load")
      const refresh = document.getElementById("rb")
      if (b) {
        spinner.style.display = "block"
        refresh.style.display = "none"
        return false // Tells App whether the Statusbar should be shown
      } else {
        spinner.style.display = "none"
        refresh.style.display = "block"
        return true
      }
    }
  }
}

</script>

<style scoped>
#title {
  text-align: center;
  margin-top: 10px;
}
#load {
  position: absolute;
  left: 48%;
  bottom: 48%;
  width: 3rem;
  height: 3rem;
}
#status {
  background: black;
  width: 100%;
  height: 100%;
}
</style>
