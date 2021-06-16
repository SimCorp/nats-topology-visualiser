<template>
<div id="app">
  <div class="structure-panel-wrapper">
    <StructurePanel ref="structurePanel" id="structurePanel" v-on:structure-node-click="onStructureNodeClick" :treeNodes="this.treenodes" v-if='dataLoaded'></StructurePanel>
  </div>
  <div class="wrapper">
    <b-spinner ref="load" id="load" label="Loading..."></b-spinner>
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
        <label class="toggle">
          <input id="toggleDarkMode" type="checkbox">
          <div id="circle"></div>
        </label>
        <Statusbar ref="statusBar" :shouldDisplay="this.showStatus" :timeOfRequest="this.timeOfRequest"></Statusbar>
          <div ref="refreshWrapper">
            <Refresh ref="refresh" @button-click="refreshGraph"/>
          </div>
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

import { BSpinner } from 'bootstrap-vue'
Vue.component('b-spinner', BSpinner)

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
    load: BSpinner,
    refreshWrapper: HTMLDivElement
  }

  servers!: ServerDatum[]
  routes!: RouteDatum[]
  clusters!: ClusterDatum[]
  gateways!: GatewayDatum[]
  leafs!: LeafDatum[]
  varz!: Varz[]
  treenodes!: TreeNode[]

  timeOfRequest: String = ""
  dataLoaded: boolean = false
  isPanelOpen: boolean = false
  showStatus: boolean = false
  renderKey: number = 0

  async mounted() {
    this.showStatus = this.displayReloadSpinner(true)
    this.dataLoaded = await this.getData()
    this.showStatus = this.displayReloadSpinner(false)

    let toggleDarkModeButton: HTMLElement | null = document.querySelector('#toggleDarkMode')

    toggleDarkModeButton?.addEventListener('click', e => this.toggleDarkMode(e))
    this.toggleDarkMode(null)
  }

  toggleDarkMode(e: MouseEvent | null) {
    let palette = [
      "primary-500",
      "neutral-800",
      "neutral-700",
      "neutral-600",
      "neutral-500",
      "neutral-400",
      "neutral-300",
      "neutral-100",
      "outlines-500",
      "error-500",
    ]

    let isNightMode = (e?.target as HTMLInputElement).checked

    console.log('isNightMode', isNightMode)
        
    palette.forEach(color => {
      document.documentElement.style.setProperty(`--color-${ color }`,
        `var(--color-${ isNightMode ? "night" : "light" }-${ color }`
      );
    })
  }

  async getData(): Promise<boolean> {
    const host = 
      (process.env.NODE_ENV === 'production')
      ? ''
      : 'https://localhost:5001'

    let mockData = true
    const data =
      (mockData)
      ? (await axios.get('./mock-data-updateeverything.json')).data
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
    const spinner = this.$refs.load
    const refresh = this.$refs.refreshWrapper
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

<style>
.b-button {
  background-color: var(--color-primary-500) !important;
  border-color: var(--color-primary-500) !important;
}
.b-button:focus {
  background-color: var(--color-primary-500) !important;
  border-color: var(--color-primary-500) !important;
  box-shadow: none !important;
}

.b-button:hover,
.b-button:visited,
.b-button:active {
  background-color: var(--color-primary-500) !important;
  border-color: var(--color-primary-500) !important;
  filter: brightness(90%) !important;
}
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
  background-color: var(--color-neutral-700);
}
.overlay-ui {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  display: flex;
  align-items: flex-start;
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
}
#load {
  position: absolute;
  left: 48%;
  bottom: 48%;
  width: 3rem;
  height: 3rem;
}

/* Toggle button */

input[type=checkbox] {
  display: none;
}

.toggle {
  margin-bottom: 0;
  height:38px;
  width: 100px;
  background-color: var(--color-primary-500);
  border-width: 5px;
  border-radius: 80px;
  /* box-shadow: 0px 4px 5px 0px rgba(0, 0, 0, 0.05); */
  display: flex;
  align-items: center;
  transition: 0.2s ease-in-out;
}

#circle {
  height: 30px;
  width: 30px;
  border-radius: 30px;
  background-color: white;
  margin: 0px 5px;
  transition: 0.2s ease-in-out;
  transform-origin: center;
  transform: rotate(-45deg);
}

input:checked .toggle {
  background-color: #444;
}

input:checked ~ #circle {
  background-color: white;
  margin-left: calc(64px + 30px /2);
  width: calc(30px /2);
  border-radius: 0px 30px 30px 0px;
  transform-origin: left center;
  transform: rotate(45deg);
}
</style>
