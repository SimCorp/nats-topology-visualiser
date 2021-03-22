<template>
<div id="app">
  <h1>Data from NATS</h1>
  <Graph :dataNATS="dataNATS"></Graph>
</div>
</template>

<script>
import axios from 'axios'
import Graph from '@/components/Graph'

export default {
  name: 'App',
  components: { Graph },
  props: {
    result: Object
  },
  data () {
    return {
      dataNATS: null // JSON object containing our NATS data
    }
  },
  mounted () {
    axios
      .get('https://localhost:5001/')
      .then(response => {
        this.dataNATS = response.data
      })
  }
}

</script>
