<template>
<div id="app">
  <p>Data result: {{ JSON.stringify(dataNATS) }}</p>
</div>
</template>

<script>
import axios from 'axios'

export default {
  name: 'App',
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
