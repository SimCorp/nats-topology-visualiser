<template>
  <div id="graph">
    <div id="visualizer"></div>
  </div>
</template>

<script>
import * as d3 from 'd3'

export default {
  name: 'Graph',
  props: {
    servers: Object,
    routes: Object
  },
  mounted () {
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
