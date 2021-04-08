<template>
  <div id='graph'>
    <Searchbar id="search" v-on:input="searchFilter"></Searchbar>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'
import Searchbar from '@/components/Searchbar.vue'

export default {
  name: 'Graph',
  components: { Searchbar },
  data () {
    return {
      searchText: '',
      servers: null,
      routes: null,
      svg: null
    }
  },
  async mounted () {
    await this.getData()

    const width = window.innerWidth
    const height = window.innerHeight
    const viewBoxScalar = 0.5

    // SVG canvas
    this.svg = d3.select('#visualizer')
      .append('div')
      .append('svg')
      // Responsive SVG needs these 2 attributes
      .attr('preserveAspectRatio', 'xMinYMin meet')
      .attr('viewBox', calculateViewBoxValue(width, height, viewBoxScalar))

    this.drawGraph(this.servers, this.routes, false)
  },
  methods: {
    // Runs every time an input is given to the search bar - searchText is the input
    searchFilter (searchText) {
      this.searchText = searchText

      if (this.searchText === '') {
        this.drawGraph(this.servers, this.routes, false)
      } else {
        this.drawGraph(this.servers, this.routes, true)
      }
    },
    // Checks whether the current server name contains the given search text/input
    isSearchMatch (serverName) {
      if (serverName === null) {
        return false
      }
      return serverName.toLowerCase().includes(this.searchText.toLowerCase()) // Boolean statement
    },
    // Fetches data from backend and updates data variables
    async getData () {
      const varzResponse = await axios.get('https://localhost:5001/nodes')
      this.servers = varzResponse.data

      const routezResponse = await axios.get('https://localhost:5001/links')
      this.routes = routezResponse.data
    },
    // Draws graph from given data
    drawGraph (servers, routes, isSearch) {
      // Clear canvas
      this.svg.selectAll('*').remove()

      // Set data to new variables (in case they get modified)
      const nodes = servers
      const links = routes

      // Physics for moving the nodes together
      const simulation = d3.forceSimulation(nodes)
        .force('link', d3.forceLink(links).id(d => d.server_id))
        .force('charge', d3.forceManyBody())
        .force('x', d3.forceX())
        .force('y', d3.forceY())

      // Connections between nodes
      const link = this.svg.append('g') // Add element g (g for group)
        .attr('stroke-opacity', 0.6)
        .selectAll('line') // Select all of type 'line'
        .data(links) // Insert the list of links
        .join('line')
        .attr('stroke', d => d.ntv_error ? '#f00' : '#999') // Set line to red, if it has an error
        .attr('stroke-width', 2)
        .style('opacity', isSearch ? 0.2 : 1.0)

      link.append('title') // Set title (hover text) for erronious link
        .text(d => d.ntv_error ? 'Something\'s Wrong' : '')

      // The nodes
      const node = this.svg.append('g') // Add element g (g for group)
        .attr('stroke', '#888')
        .attr('stroke-width', 3)
        .selectAll('circle') // Select all of type 'circle'
        .data(nodes)
        .join('circle')
        // Set the placement and radius for each node
        .attr('cx', () => { return Math.random() }) // Random because, then the simulation can move them around
        .attr('cy', () => { return Math.random() })
        .attr('r', 5)
        .attr('fill', d => d.ntv_error ? '#f00' : '#000') // Make node red if it has error
        .style('opacity', d => this.isSearchMatch(d.server_name) || (d.server_name === null && !isSearch) ? 1.0 : 0.2)
        .call(drag(simulation)) // Handle dragging of the nodes
        .on('click', (d, i) => { // Log the value of the chosen node on click
          axios.get('https://localhost:5001/varz/' + i.server_id).then(a => {
            console.log(d)
            console.log(i)
            console.log(a.data)
          })
        })

      // Set a title on the node, which is shown when hovered
      node.append('title')
        .text(d => (d.ntv_error ? '[Crashed?] \n' : '') + 'NAME:' + d.server_name + '\nID:' + d.server_id)

      // What it does whenever the canvas updates
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
}

function calculateViewBoxValue (width: number, height: number, viewBoxScalar: number) {
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

</script>

<style>
/* Puts the canvas behind all other HTML elements */
svg {
  position: absolute;
  left: 0;
  top: 0;
  z-index: -1;
}
</style>
