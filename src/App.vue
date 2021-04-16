<template>
  <b-container class="column">
    <div id="app" class="column">
      <h1>Visualizer</h1>
      <Graph/>
    </div>
    <div>
      <Statusbar/>
    </div>
  </b-container>
</template>

<script lang="ts">
import Graph from '@/components/Graph/Graph.vue'
import Statusbar from '@/components/Statusbar.vue'

export default {
  name: 'App',
  components: { Graph, Statusbar }
}

</script>
