<template>
  <div id="app">
    <h1>Visualizer</h1>
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
