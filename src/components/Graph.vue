<template>
  <div id='graph'>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'
import processData from '../helpers/ProcessData'

function calulateViewBoxValue (width: number, height: number, viewBoxScalar: number) {
  const viewBoxTop = -width / 2
  const viewBoxLeft = -height / 2
  const viewBoxRight = width
  const viewBoxBottom = height
  const viewBoxValue = `${viewBoxTop * viewBoxScalar}, ${viewBoxLeft * viewBoxScalar}, ${viewBoxRight * viewBoxScalar}, ${viewBoxBottom * viewBoxScalar}`
  return viewBoxValue
}

const drag = (simulation: any) => {
  function dragstarted (event: any, d: any) {
    if (!event.active) simulation.alphaTarget(0.3).restart()
    d.fx = d.x
    d.fy = d.y
  }
  function dragged (event: any, d: any) {
    d.fx = event.x
    d.fy = event.y
  }
  function dragended (event: any, d: any) {
    if (!event.active) simulation.alphaTarget(0)
    d.fx = null
    d.fy = null
  }
  return d3.drag()
    .on('start', dragstarted)
    .on('drag', dragged)
    .on('end', dragended)
}

export default {
  name: 'Graph',
  props: {
  },
  async mounted () {
    const width = 1000
    const height = 800
    const viewBoxScalar = 0.5

    const varzResponse = await axios.get('https://localhost:5001/')
    const servers = varzResponse.data

    const routezResponse = await axios.get('https://localhost:5001/routez')
    const routes = routezResponse.data

    // Process data
    const { processedServers, processedRoutes } = processData({ servers, routes })

    // d3 canvas
    const nodes = processedServers.map(d => Object.create(d))
    const links = processedRoutes.map(d => Object.create(d))

    const simulation = d3.forceSimulation(nodes)
      .force('link', d3.forceLink(links).id(d => d.server_id))
      .force('charge', d3.forceManyBody())
      .force('x', d3.forceX())
      .force('y', d3.forceY())

    const svg = d3.select('#visualizer')
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .attr('viewBox', calulateViewBoxValue(width, height, viewBoxScalar))

    const link = svg.append('g')
      .attr('stroke-opacity', 0.6)
      .selectAll('line')
      .data(links)
      .join('line')
      .attr('stroke', d => d.ntv_error ? '#f00' : '#999')
      .attr('stroke-width', 2)

    link.append('title')
      .text(d => d.ntv_error ? 'Something\'s Wrong' : '')

    const node = svg.append('g')
      .attr('stroke', '#888')
      .attr('stroke-width', 3)
      .selectAll('circle')
      .data(nodes)
      .join('circle')
      .attr('cx', () => { return Math.random() * width })
      .attr('cy', () => { return Math.random() * height })
      .attr('r', 5)
      .attr('fill', d => d.ntv_error ? '#f00' : '#000')
      .call(drag(simulation))
      .on('click', (d, i) => {
        console.log(i.server_id)
        console.log(d)
        console.log(i)
      })

    node.append('title')
      .text(d => (d.ntv_error ? '[Crashed?] \n' : '') + 'NAME:' + d.server_name + '\nID:' + d.server_id)

    simulation.on('tick', () => {
      link
        .attr('x1', d => d.source.x)
        .attr('y1', d => d.source.y)
        .attr('x2', d => d.target.x)
        .attr('y2', d => d.target.y)

      node
        .attr('cx', d => d.x)
        .attr('cy', d => d.y)
    })
  }
}

</script>

<style scoped>

</style>
