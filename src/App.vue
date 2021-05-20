<template>
<div id="app">
  <h2 id="title">Topology Visualizer</h2>
  <Graph
    ref="graph"
    @node-click="onNodeClick"
    v-if='dataLoaded'
    :servers='this.servers'
    :routes='this.routes'
    :clusters='this.clusters'
    :gateways='this.gateways'
    :leafs='this.leafs'
    :dataLoaded='this.dataLoaded'
  >
  </Graph>
  <Searchbar ref="search" id="search" v-on:input="onSearchInput" @button-click="onSearchReset"/>
  <InfoPanel ref="panel"></InfoPanel>
  <Statusbar></Statusbar>
  <StructurePanel ref="structurepanel" id="structurePanel" v-on:structure-node-click="onStructureNodeClick"></StructurePanel>
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
import Searchbar from '@/components/Searchbar.vue'
import StructurePanel from '@/components/StructurePanel.vue';

import Zoombar from '@/components/Zoombar.vue'
import LinkDatum from './components/Graph/LinkDatum'
import TreeList from '@/components/TreeList.vue'

export default {
  name: 'App',
  components: {
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
    leafs: RouteDatum[]; // TODO add LeafDatum?
    dataLoaded: boolean;
    isPanelOpen: boolean;
  } {
    return {
      servers: [],
      routes: [],
      clusters: [],
      gateways: [],
      leafs: [],
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
      this.gateways = (await axios.get(`${host}/gatewayLinks`)).data
      this.leafs = (await axios.get(`${host}/leaflinks`)).data

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

    onStructureNodeClick ({name, server_id}){
      var nameStr = name.toString()
      var idStr = server_id.toString()
      //console.log(nameStr)
      this.$refs.graph.searchFilter(nameStr)
      this.$refs.search.changeText(nameStr)


      axios.get('https://localhost:5001/varz/' + idStr).then(a => {
            this.onNodeClick({nodeData: a.data, id: idStr})
      })

    }

  }
}

</script>

<style scoped>
#title {
  text-align: center;
  margin-top: 10px;
}


</style>
