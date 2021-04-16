<template>
<div id="app">
  <h1 id="title">Topology Visualizer</h1>
  <Graph/>
  <Statusbar/>
</div>
</template>

<script lang="ts">
import Graph from '@/components/Graph/Graph.vue'
import Statusbar from '@/components/Statusbar.vue'

export default {
  name: 'App',
  components: { Graph, Statusbar }
}

</script>

<style scoped>
#title {
  text-align: center;
  margin-top: 15px;
}
</style>
