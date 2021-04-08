<template>
<div id="app">
  <h1>Visualizer</h1>
  <Graph/>
</div>
</template>

<script lang="ts">
import Graph from '@/components/Graph.vue'

export default {
  name: 'App',
  components: { Graph }
}

</script>
