<template>
<div id="app">
  <h1>Data from NATS</h1>
  <Graph :servers="servers" :routes="routes"></Graph>
</div>
</template>

<script>
import axios from 'axios'
import Graph from '@/components/Graph'

export default {
  name: 'App',
  components: { Graph },
  props: {
    servers: Object,
    routes: Object
  },
  mounted () {
    axios
      .get('https://localhost:5001/')
      .then(response => {
        this.servers = response.data
      })
    axios
      .get('https://localhost:5001/routez')
      .then(response => {
        this.routes = response.data
      })
  }
}

</script>
