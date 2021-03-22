<template>
  <div id="graph">
    <p id="test">Color test</p>
    <ul id="example-1">
      <li v-for="server in dataNATS" :key="server[0]">
        {{ server.server_name }}
      </li>
    </ul>
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
