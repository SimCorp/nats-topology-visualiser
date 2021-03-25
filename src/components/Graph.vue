<template>
  <div id="graph">
    <div id="visualizer"></div>
  </div>
</template>

<script lang="ts">
import * as d3 from 'd3'
import axios from 'axios'

export default {
  name: 'Graph',
  props: {
    servers: Array,
    routes: Array
  },
  async mounted () {
    const response = await axios.get('https://localhost:5001/')
    // this.$emit('update:servers', response.data)
    this.servers = response.data
    const width = 1000
    const height = 800

    const svg = d3.select('#visualizer')
      .append('svg')
      .attr('width', width)
      .attr('height', height)

    const node = svg.append('g')
      .selectAll('nodes')
      .data(this.servers)
      .enter()
    console.log(node)

    const circles = node.append('circle')
      .attr('cx', () => { return Math.random() * width })
      .attr('cy', () => { return Math.random() * height })
      .attr('r', 40)
      .style('fill', '#69b3a2')
  }
}
</script>

<style scoped>

</style>
