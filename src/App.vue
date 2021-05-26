<template>
<div id="app">
  <div class="structure-panel-wrapper">
    <StructurePanel ref="structurePanel" id="structurePanel" v-on:structure-node-click="onStructureNodeClick" :treeNodes="this.treenodes" v-if='dataLoaded'></StructurePanel>
  </div>
  <div class="wrapper">
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
    <div class="overlay-ui">
      <Searchbar ref="searchBar" id="search" v-on:input="onSearchInput" @button-click="onSearchReset"></Searchbar>
      <div id="status">
        <Statusbar ref="statusBar" :shouldDisplay="this.showStatus" :timeOfRequest="this.timeOfRequest"></Statusbar>
        <Refresh ref="refresh" @button-click="refreshGraph"/>
      </div>
    </div>
    <InfoPanel ref="infoPanel"></InfoPanel>
  </div>
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
import Vue from 'vue'
import Component from 'vue-class-component'
import {id} from "postcss-selector-parser";
import TreeNode from './components/TreeNode'

@Component({
  components: {
    Refresh,
    Graph,
    Statusbar,
    Searchbar,
    InfoPanel,
    StructurePanel,
  }
})
export default class App extends Vue {
  $refs!: {
    refresh: Refresh,
    graph : Graph,
    statusBar : Statusbar,
    searchBar : Searchbar,
    infoPanel : InfoPanel,
    structurePanel : StructurePanel,
  }

  servers!: ServerDatum[]
  routes!: RouteDatum[]
  clusters!: ClusterDatum[]
  gateways!: GatewayDatum[]
  leafs!: LeafDatum[]
  varz!: Varz[]
  timeOfRequest!: String
  treenodes!: TreeNode[]

  dataLoaded: boolean = false
  isPanelOpen: boolean = false
  showStatus: boolean = false
  renderKey: number = 0

  async mounted() {
    this.showStatus = this.$refs.refresh.displayReloadSpinner(true)
    this.dataLoaded = await this.getData()
    this.showStatus = this.$refs.refresh.displayReloadSpinner(false)
  }

  async getData(): Promise<boolean> {
    const host = 
      (process.env.NODE_ENV === 'production')
      ? ''
      : 'https://localhost:5001'

    let mockData = true
    const data =
      (mockData)
      ? (await (await fetch('./mock-data-updateeverything.json')).json())
      : (await axios.get(`${host}/api/updateEverything`)).data

    this.servers = data.processedServers
    this.routes = data.links
    this.clusters = data.processedClusters
    this.gateways = data.gatewayLinks
    this.leafs = data.leafLinks
    this.varz = data.varz
    this.treenodes = data.treeNodes
    this.timeOfRequest = data.timeOfRequest
    return true
  }

  onNodeClick ({nodeData, id}: {nodeData: Varz | string, id: string}) {
    this.$refs.infoPanel.onNodeClick(nodeData, id)
  }

  onSearchInput (text: string) {
    this.$refs.graph.searchFilter(text)
  }

  onSearchReset () {
    this.$refs.graph.searchReset()
  }

  getServerWithId(server_id: string): Varz | string {
    for (const server of this.varz) {
      if (server.server_id === server_id) return server // TODO add varz type
    }
    return ''
  }

  onStructureNodeClick ({name, server_id}: { name: string; server_id: number }) {
    var nameStr = name.toString()
    var idStr = server_id.toString()
    this.$refs.graph.searchFilter(nameStr)
    this.$refs.searchBar.changeText(nameStr)
    this.onNodeClick({nodeData: this.getServerWithId(idStr), id: idStr})
  }

  async refreshGraph () {
    const refresh = this.$refs.refresh
    refresh.displayRefreshSpinner(true)
    this.dataLoaded = await this.getData()
    refresh.displayRefreshSpinner(false)
    this.renderKey += 1 // Tells the Graph component to completely reload
  }
  
  displayReloadSpinner (b: boolean) { // Used when reloading the page (F5)
    const spinner = document.getElementById("load")
    const refresh = document.getElementById("rb")
    if (b) {
      spinner!.style.display = "block"
      refresh!.style.display = "none"
      return false // Tells App whether the Statusbar should be shown
    } else {
      spinner!.style.display = "none"
      refresh!.style.display = "block"
      return true
    }
  }
}

</script>

<style scoped>
#app {
  height: 100vh;
  width: 100%;
  display: flex;
  flex-direction: row;
}
.wrapper {
  flex: 1;
  position: relative;
  min-width: 490px;
  height: 100%;
  width: 100%;
}
.structure-panel-wrapper {
  width: 320px;
  background-color: rgb(248, 249, 250);
}
.overlay-ui {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 1em;
  pointer-events: none;
}
.overlay-ui > * {
  pointer-events: auto;
}
#status {
  display: flex;
  flex-direction: row;
  gap: 0.4em;
  width: 0px;
}
#load {
  position: absolute;
  left: 48%;
  bottom: 48%;
  width: 3rem;
  height: 3rem;
}
</style>
