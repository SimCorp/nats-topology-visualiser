﻿<template>
  <div id='graph'>
    <Searchbar id="search" v-on:input="searchFilter"></Searchbar>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'
import Searchbar from '@/components/Searchbar.vue'
import RouteDatum from './RouteDatum'
import ServerDatum from './ServerDatum'
import ClusterDatum from './ClusterDatum'
import { D3DragEvent, SimulationNodeDatum, Selection, SubjectPosition } from 'd3'

export default {
  name: 'Graph',
  components: { Searchbar },
  data (): { 
    searchText: string; 
    servers: ServerDatum[]; 
    routes: RouteDatum[];
    clusters: ClusterDatum[];
    svg: Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null;
  } {
    return {
      searchText: '',
      servers: [],
      routes: [],
      clusters: [],
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

    this.drawGraph(false)
  },
  methods: {
    // Runs every time an input is given to the search bar - searchText is the input
    searchFilter (searchText: string) {
      this.searchText = searchText

      if (this.searchText === '') {
        this.drawGraph(false)
      } else {
        this.drawGraph(true)
      }
    },
    // Checks whether the current server name contains the given search text/input
    isSearchMatch (serverName: string) {
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

      const clusterz = await axios.get('https://localhost:5001/clusters')
      this.clusters = clusterz.data
    },
    // Draws graph from given data
    drawGraph (isSearch: boolean) {
      // Clear canvas
      this.svg?.selectAll('*').remove()

      // Set data to new variables (in case they get modified)
      const nodes = this.servers
      const links = this.routes
      const hulls = this.clusters

      // A convex hull (enclosing path) for clusters
      const hull = this.svg?.append('g')
        .selectAll('path')
        .data(hulls)
        .enter()
        .append('path')
        .attr('d', '')
        .attr('opacity', 0.7)
        .attr('stroke', '#ddd')
        .attr('stroke-width', '13px')
        .attr('stroke-linejoin', 'round')
        .style('fill', '#ddd')
      hull?.append('title')
        .text(d => d.name)


      // Physics for moving the nodes together
      const simulation: d3.Simulation<ServerDatum, RouteDatum> = d3.forceSimulation(nodes)
        .force('link', d3.forceLink<ServerDatum, RouteDatum>(links).id(d => d.server_id))
        .force('charge', d3.forceManyBody())
        .force('x', d3.forceX())
        .force('y', d3.forceY())

      // Connections between nodes
      const link = this.svg?.append('g') // Add element g (g for group)
        .attr('stroke-opacity', 0.6)
        .selectAll('line') // Select all of type 'line'
        .data(links) // Insert the list of links
        .join('line')
        .attr('stroke', d => d.ntv_error ? '#f00' : '#999') // Set line to red, if it has an error
        .attr('stroke-width', 2)
        .style('opacity', isSearch ? 0.2 : 1.0)

      link?.append('title') // Set title (hover text) for erronious link
        .text(d => d.ntv_error ? 'Something\'s Wrong' : '')

      // The nodes
      const node = this.svg?.append('g') // Add element g (g for group)
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
      node?.append('title')
        .text(d => (d.ntv_error ? '[Crashed?] \n' : '') + 'NAME:' + d.server_name + '\nID:' + d.server_id)

      // What it does whenever the canvas updates
      simulation.on('tick', () => {
        link?.attr('x1', d => d.source.x)
          .attr('y1', d => d.source.y)
          .attr('x2', d => d.target.x)
          .attr('y2', d => d.target.y)

        node?.attr('cx', d => d.x)
          .attr('cy', d => d.y)

        hull?.attr('d', d => {
          // TODO: Find a more efficient method (pre process groupings of nodes according to clusters)
          const nodesOfCluster = d.servers.map(serverDatum => {
            return nodes.filter(serverNode => (serverDatum.server_id === serverNode.server_id))
          })
          // Create SVG path from coordinates
          const nodesCoords: [number, number][] = nodesOfCluster.map(node => {
            return [node[0].x, node[0].y]
          })
          const hullCoords: [number, number][] | null = d3.polygonHull(nodesCoords)
          return svgPath(hullCoords || nodesCoords) // Polygonhull returns null for 2 or fewer nodes. 
        })
      })
    }
  }
}

function svgPath (coordinates: [number, number][]): string {
  // Create closed SVG path from coordinates
  const initialValue = ''
  const hullPath: string = coordinates?.reduce((str, coords, index, array) =>
    index === 0
      ? `${str} M ${coords[0]},${coords[1]}` // Initially use MoveTo command 'M'
      : `${str} L ${coords[0]},${coords[1]}` // otherwise use LineTo command 'L'
  , initialValue) + ' Z' // End with ClosePath command 'Z'
  return hullPath
}

function calculateViewBoxValue (width: number, height: number, viewBoxScalar: number) {
  const viewBoxTop = -width / 2
  const viewBoxLeft = -height / 2
  const viewBoxRight = width
  const viewBoxBottom = height
  const viewBoxValue = `${viewBoxTop * viewBoxScalar}, ${viewBoxLeft * viewBoxScalar}, ${viewBoxRight * viewBoxScalar}, ${viewBoxBottom * viewBoxScalar}`
  return viewBoxValue
}

function drag (simulation: d3.Simulation<ServerDatum, RouteDatum>): d3.DragBehavior<Element, ServerDatum, ServerDatum | SubjectPosition> & ((this: Element, event: any, d: ServerDatum) => void) {
  function dragstarted (event: D3DragEvent<SVGElement, SimulationNodeDatum, ServerDatum>, d: ServerDatum) { 
    if (!event.active) simulation.alphaTarget(0.3).restart()
    d.fx = d.x
    d.fy = d.y
  }
  function dragged (event: D3DragEvent<SVGElement, SimulationNodeDatum, ServerDatum>, d: ServerDatum) {
    d.fx = event.x
    d.fy = event.y
  }
  function dragended (event: D3DragEvent<SVGElement, SimulationNodeDatum, Node>, d: ServerDatum) {
    if (!event.active) simulation.alphaTarget(0)
    d.fx = null
    d.fy = null
  }
  return d3.drag<Element, ServerDatum>()
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