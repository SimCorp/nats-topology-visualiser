<template>
  <div id='graph'>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'

export default {
  name: 'Graph',
  props: {

  },
  async mounted () {
    /* eslint-disable @typescript-eslint/camelcase */
    const width = 1000
    const height = 800
    const viewboxdims = 0.5

    const varzResponse = await axios.get('https://localhost:5001/')
    const servers = varzResponse.data

    const routezResponse = await axios.get('https://localhost:5001/routez')
    const routes = routezResponse.data

    // Process data

    const processedServers = servers
    // Hacky patch for a missing node from varz nodes
    processedServers.push({
      server_id: 'NCJSE2JXMGGWA2UTKDPEZSY56B6BS3QCMLWE7KDE5MSOQDQA5FBL3TQO'
    })

    interface Link {
      source: string;
      target: string;
    }
    const processedRoutes = []
    routes.forEach((server: any) => {
      const source: string = server.server_id
      server.routes.forEach((route: any) => {
        const target: string = route.remote_id
        processedRoutes.push({
          source: source,
          target: target
        })
      })
    })

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
      .attr('viewBox', `${-width / 2 * viewboxdims}, ${-height / 2 * viewboxdims}, ${width * viewboxdims}, ${height * viewboxdims}`)

    const node = svg.append('g')
      .attr('stroke', '#888')
      .attr('stroke-width', 3)
      .selectAll('circle')
      .data(nodes)
      .join('circle')
      .attr('cx', () => { return Math.random() * width })
      .attr('cy', () => { return Math.random() * height })
      .attr('r', 5)
      .attr('fill', '#000')

    const link = svg.append('g')
      .attr('stroke', '#999')
      .attr('stroke-opacity', 0.6)
      .selectAll('line')
      .data(links)
      .join('line')
      .attr('stroke-width', 2)

    node.append('title')
      .text(d => d.server_id)

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
