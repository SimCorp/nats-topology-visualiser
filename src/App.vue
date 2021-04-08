<template>
<div id="app">
  <h1 id="header">Visualizer</h1>
    <Graph/>
</div>
</template>

<script lang="ts">
import Graph from '@/components/Graph/Graph.vue'

export default {
  name: 'App',
  components: { Graph }
}

</script>
