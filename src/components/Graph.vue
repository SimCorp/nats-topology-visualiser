<template>
  <div id="graph">
    <p id="test">hello</p>
    <p>{{ JSON.stringify(dataNATS) }}</p>
  </div>
</template>

<script>
import * as d3 from 'd3'

export default {
  name: 'Graph',
  props: {
    dataNATS: Object
  },
  mounted () {
    d3.select('#test').style('color', 'green')
  }
}
</script>

<style scoped>

</style>
