<template>
  <div id='graph'>
    <Searchbar id="search" v-on:input="searchFilter" @button-click="searchReset"/>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'
import Searchbar from '@/components/Searchbar.vue'
import GatewayDatum from './GatewayDatum'
import ServerDatum from './ServerDatum'
import ClusterDatum from './ClusterDatum'
import { D3DragEvent, SimulationNodeDatum, Selection, SubjectPosition } from 'd3'
import LinkDatum from './LinkDatum'
import RouteDatum from './RouteDatum'

export default {
  name: 'Graph',
  components: { Searchbar },
  data (): { 
    searchText: string;
    servers: ServerDatum[]; 
    routes: RouteDatum[];
    clusters: ClusterDatum[];
    gateways: GatewayDatum[];
    svg: Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null;
  } {
    return {
      servers: [],
      routes: [],
      clusters: [],
      gateways: [],
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

    this.drawGraph()
    this.searchFilter('')
  },
  methods: {
    // Runs every time an input is given to the search bar - searchText is the input
    // Checks whether the current server name contains the given search text/input
    // Updates isSearchMatch field of datums
    searchFilter (searchText: string) {
      const isEmptySearch = searchText === ''

      console.log('isEmptySearch', isEmptySearch)

      searchText = searchText.toLowerCase();

      this.servers.forEach(server => {
        server.isSearchMatch = isSearchMatch()

        function isSearchMatch () {
          const serverName = server?.server_name?.toLowerCase()
          if (serverName === null || serverName === undefined)
            return isEmptySearch

          return isEmptySearch || (serverName.includes(searchText))
        }
      });

      this.gateways.forEach(gateway => {
        gateway.isSearchMatch = isEmptySearch
      });

      this.routes.forEach(route => {
        route.isSearchMatch = isEmptySearch
      });

      this.drawGraph()
    },
    // Resets the graph on click of the "X" button
    searchReset() {
      this.searchText = '' // This is only Graph's local variable, the actual input text gets removed in Searchbar
      this.drawGraph(false)
    },
    async getData () {
      const host = 'https://localhost:5001'

      const varzResponse = await axios.get(`${host}/nodes`)
      this.servers = varzResponse.data

      const routezResponse = await axios.get(`${host}/links`)
      this.routes = routezResponse.data

      const clusterz = await axios.get(`${host}/clusters`)
      this.clusters = clusterz.data

      const gatewayLinks = await axios.get(`${host}/gatewayLinks`)
      this.gateways = gatewayLinks.data
    },
    // Draws graph from given data
    drawGraph () {
      // Clear canvas
      this.svg?.selectAll('*').remove()

      // Local variables
      const svg = this.svg
      const servers = this.servers
      const routes = this.routes
      const clusters = this.clusters
      const gateways = this.gateways

      // Cluster Map
      const clusterNameToCluster = new Map<string, ClusterDatum>()
      this.clusters.forEach(cluster => {
        clusterNameToCluster.set(cluster.name, cluster)
      })

      // Put every kind of node in a collection, before passing to simulation
      const allNodes: SimulationNodeDatum[] = (servers as SimulationNodeDatum[]).concat(clusters)

      // Physics for moving the nodes together
      const simulation: d3.Simulation<SimulationNodeDatum, LinkDatum<SimulationNodeDatum>> = d3.forceSimulation(allNodes)
        .force('link', d3.forceLink<ServerDatum, RouteDatum>(routes).id(d => d.server_id))
        .force('link', d3.forceLink<ClusterDatum, GatewayDatum>(gateways).id(d => d.name).strength(0.4).distance(50))
        .force('charge', d3.forceManyBody())
        .force('x', d3.forceX())
        .force('y', d3.forceY())

      // Gateways
      const gatewayLink = createGatewayLinkSelection(svg, gateways)
      const hull = createHullSelection(svg, clusters, simulation)
      const cluster = createClusterNodeSelection(svg, clusters, simulation)
      const routeLink = createRouteLinkSelection(svg, routes)
      const serverNode = createServerNodeSelection(svg, servers, simulation)

      // Update data on simulation tick
      simulation.on('tick', simulationTick)

      function simulationTick (): void {
        serverNode?.attr('cx', d => d.x)
          .attr('cy', d => d.y)

        routeLink?.attr('x1', d => d.source.x)
          .attr('y1', d => d.source.y)
          .attr('x2', d => d.target.x)
          .attr('y2', d => d.target.y)

        const k = simulation.alpha() * 0.3;
        servers.forEach(serverNode => {
          const cluster = clusterNameToCluster.get(serverNode.ntv_cluster)
          serverNode.y += (cluster!.y - serverNode.y) * k;
          serverNode.x += (cluster!.x - serverNode.x) * k;
        })

        cluster?.attr('cx', d => d.x)
          .attr('cy', d => d.y)

        hull?.attr('d', d => getHullPath(d, servers))

        gatewayLink?.attr('x1', d => d.source.x)
          .attr('y1', d => d.source.y)
          .attr('x2', d => d.target.x)
          .attr('y2', d => d.target.y)
      }
    }
  }
}

function createGatewayLinkSelection (
  svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
  gateways: GatewayDatum[]
) {
  const gatewayLink = svg?.append('g') // Add element g (g for group)
  .attr('stroke-opacity', 0.6)
  .selectAll('line') // Select all of type 'line'
  .data(gateways) // Insert the list of links
  .join('line')
  .attr('stroke', d => d.ntv_error ? '#f00' : '#999') // Set line to red, if it has an error
  .attr('stroke-width', 3)
  .style('opacity', d => d.isSearchMatch ? 1.0 : 0.2)

  gatewayLink?.append('title')
    .text(d => d.errorsAsString)

  return gatewayLink
}

// A convex hull (enclosing path) for clusters
function createHullSelection (
  svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
  clusters: ClusterDatum[],
  simulation: d3.Simulation<d3.SimulationNodeDatum, LinkDatum<d3.SimulationNodeDatum>>
) {
  const hull = svg?.append('g')
    .selectAll('path')
    .data(clusters)
    .enter()
    .append('path')
    .attr('d', '')
    .attr('stroke', '#ddd')
    .attr('stroke-width', '13px')
    .attr('stroke-linejoin', 'round')
    .attr('stroke-width', 20)
    .style('fill', '#ddd')
    .call(drag(simulation))

  hull?.append('title')
    .text(d => d.name)

  return hull
}

function createClusterNodeSelection (
  svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
  clusters: ClusterDatum[],
  simulation: d3.Simulation<d3.SimulationNodeDatum, LinkDatum<d3.SimulationNodeDatum>>
) {
  return svg?.append('g') // Add element g (g for group)
    .selectAll('circle') // Select all of type 'circle'
    .data(clusters)
    .join('circle')
    // Set the placement and radius for each node
    .attr('cx', () => { return Math.random() * 300 - 150 }) // Random because, then the simulation can move them around
    .attr('cy', () => { return Math.random() * 300 - 150 })
    .attr('r', 1)
    .style('opacity', 0)
    .call(drag(simulation)) // Handle dragging of the nodes
}

function createRouteLinkSelection (
  svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
  routes: RouteDatum[]
) {
  const routeLink = svg?.append('g') // Add element g (g for group)
    .attr('stroke-opacity', 0.6)
    .selectAll('line') // Select all of type 'line'
    .data(routes) // Insert the list of links
    .join('line')
    .attr('stroke', d => d.ntv_error ? '#f00' : '#999') // Set line to red, if it has an error
    .attr('stroke-width', 2)
    .style('opacity', d => d.isSearchMatch ? 1.0 : 0.2)

  routeLink?.append('title') // Set title (hover text) for erronious link
    .text(d => d.ntv_error ? 'Something\'s Wrong' : '')
  
  return routeLink
}

function createServerNodeSelection (
  svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
  servers: ServerDatum[],
  simulation: d3.Simulation<d3.SimulationNodeDatum, LinkDatum<d3.SimulationNodeDatum>>
) {
  const serverNode = svg?.append('g') // Add element g (g for group)
    .attr('stroke', '#888')
    .attr('stroke-width', 3)
    .selectAll('circle') // Select all of type 'circle'
    .data(servers)
    .join('circle')
    // Set the placement and radius for each node
    .attr('cx', () => { return Math.random() }) // Random because, then the simulation can move them around
    .attr('cy', () => { return Math.random() })
    .attr('r', 5)
    .attr('fill', d => d.ntv_error ? '#f00' : '#000') // Make node red if it has error
    .style('opacity', d => d.isSearchMatch ? 1.0 : 0.2)
    .call(drag(simulation)) // Handle dragging of the nodes
    .on('click', (d, i) => { // Log the value of the chosen node on click
      axios.get('https://localhost:5001/varz/' + i.server_id).then(a => {
        console.log(d)
        console.log(i)
        console.log(a.data)
      })
    })

  // Set a title on the node, which is shown when hovered
  serverNode?.append('title')
    .text(d => (d.ntv_error ? '[Crashed?] \n' : '') + 'NAME:' + d.server_name + '\nID:' + d.server_id)

  return serverNode
}

function getHullPath (cluster: ClusterDatum, servers: ServerDatum[]) {
  // TODO: Find a more efficient method (pre process groupings of nodes according to clusters)
  const nodesOfCluster = cluster.servers.map(serverDatum => {
    return servers.filter(serverNode => (serverDatum.server_id === serverNode.server_id))
  })
  // Create SVG path from coordinates
  const nodesCoords: [number, number][] = nodesOfCluster.map(node => {
    return [node[0].x, node[0].y]
  })
  nodesCoords.push([cluster.x, cluster.y])
  const hullCoords: [number, number][] | null = d3.polygonHull(nodesCoords)

  return svgPath(hullCoords || nodesCoords) // Polygonhull returns null for 2 or fewer nodes. 
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

function drag (simulation: d3.Simulation<SimulationNodeDatum, LinkDatum<SimulationNodeDatum>>): d3.DragBehavior<Element, ServerDatum, ServerDatum | SubjectPosition> & ((this: Element, event: any, d: ServerDatum) => void) {
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

function calculateViewBoxValue (width: number, height: number, viewBoxScalar: number) {
  const viewBoxTop = -width / 2
  const viewBoxLeft = -height / 2
  const viewBoxRight = width
  const viewBoxBottom = height
  const viewBoxValue = `${viewBoxTop * viewBoxScalar}, ${viewBoxLeft * viewBoxScalar}, ${viewBoxRight * viewBoxScalar}, ${viewBoxBottom * viewBoxScalar}`
  return viewBoxValue
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
