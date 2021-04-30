<template>
  <div id='graph'>
    <Searchbar id="search" v-on:input="searchFilter" @button-click="searchReset"/>
    <div id="zoomButtons">
      <b-button class="zoomButtons" @click="zoomOut" variant="info">-</b-button>
      <b-button class="zoomButtons" @click="zoomIn" variant="info">+</b-button>
      <label v-model="text"> {{ this.zoomLevel }}% </label>
    </div>
    <v-zoomer id="zoomer" ref="zoom" maxScale="6" minScale="1">
    <div id='visualizer'></div>
    </v-zoomer>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import GatewayDatum from './GatewayDatum'
import ServerDatum from './ServerDatum'
import ClusterDatum from './ClusterDatum'
import { D3DragEvent, Selection, SubjectPosition } from 'd3'
import LinkDatum from './LinkDatum'
import RouteDatum from './RouteDatum'

import axios from 'axios'
import { PropType } from 'vue'
import NodeDatum from './NodeDatum'

export default {
  name: 'Graph',
  props: {
    servers: Array as PropType<ServerDatum[]>,
    routes: Array as PropType<RouteDatum[]>,
    clusters: Array as PropType<ClusterDatum[]>,
    gateways: Array as PropType<GatewayDatum[]>,
    leafs: Array as PropType<RouteDatum[]>
  },
  data (): {
    svg: Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null;
  } {
    return {
      svg: null,
      zoomLevel: 100,
    }
  },
  mounted () {
    const width = window.innerWidth
    const height = window.innerHeight
    const viewBoxScalar = 0.5

    // SVG canvas
    this.svg = d3.select('#visualizer')
      .append('div')
      .append('svg')
      // Responsive SVG needs these 2 attributes
      .attr('preserveAspectRatio', 'xMinYMin meet')
      .attr('viewBox', this.calculateViewBoxValue(width, height, viewBoxScalar))

    this.svg.append('g').attr('id', 'gateways')
    this.svg.append('g').attr('id', 'hulls')
    this.svg.append('g').attr('id', 'clusters')
    this.svg.append('g').attr('id', 'routes')
    this.svg.append('g').attr('id', 'leafs')
    this.svg.append('g').attr('id', 'servers')

    this.drawGraph()
    this.searchFilter('')
  },
  methods: {
    // Runs every time an input is given to the search bar - searchText is the input
    // Checks whether the current server name contains the given search text/input
    // Updates isSearchMatch field of datums
    searchFilter (searchText: string): void {
      const isEmptySearch = searchText === ''

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
      this.searchFilter('')
      this.drawGraph()
    },
    // Draws graph from given data
    drawGraph (): void {
      const svg = this.svg
      const servers = this.servers
      const routes = this.routes
      const clusters = this.clusters
      const gateways = this.gateways
      const leafs = this.leafs

      // Cluster Map for fast lookup
      const clusterNameToCluster = new Map<string, ClusterDatum>()
      this.clusters.forEach(cluster => {
        clusterNameToCluster.set(cluster.name, cluster)
      })

      // Put every kind of node in a collection, before passing to simulation
      const allNodes: NodeDatum[] = (servers as NodeDatum[]).concat(clusters)

      // Physics for moving the nodes together
      const simulation: d3.Simulation<NodeDatum, LinkDatum<NodeDatum>> = d3.forceSimulation(allNodes)
        .force('link', d3.forceLink<ServerDatum, RouteDatum>(routes).id(d => d.server_id))
        .force('link', d3.forceLink<ClusterDatum, GatewayDatum>(gateways).id(d => d.name).strength(0.4).distance(50))
        .force('link', d3.forceLink<ServerDatum, RouteDatum>(leafs).id(d => d.server_id).strength(0.01).distance(200))
        .force('charge', d3.forceManyBody())
        .force('x', d3.forceX())
        .force('y', d3.forceY())

      // // Gateways
      const gatewayLink = this.createGatewayLinkSelection(svg, gateways)
      const hull = this.createHullSelection(svg, clusters, simulation)
      const cluster = this.createClusterNodeSelection(svg, clusters, simulation)
      const routeLink = this.createRouteLinkSelection(svg, routes)
      const leafLink = this.createLeafLinkSelection(svg, leafs)
      const serverNode = this.createServerNodeSelection(svg, servers, simulation)

      // Update data on simulation tick
      simulation.on('tick', () => {
        serverNode?.attr('cx', d => d.x)
          .attr('cy', d => d.y)

        routeLink?.attr('x1', d => d.source.x)
          .attr('y1', d => d.source.y)
          .attr('x2', d => d.target.x)
          .attr('y2', d => d.target.y)

        leafLink?.attr('x1', d => d.source.x)
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

        hull?.attr('d', d => this.getHullPath(d, servers))

        gatewayLink?.attr('x1', d => d.source.x)
          .attr('y1', d => d.source.y)
          .attr('x2', d => d.target.x)
          .attr('y2', d => d.target.y)
      })
    },

    createGatewayLinkSelection (
      svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
      gateways: GatewayDatum[]
    ) {
      const gatewayLink = svg?.select('g#gateways') // Add element g (g for group)
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
    },

    // A convex hull (enclosing path) for clusters
    createHullSelection (
      svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
      clusters: ClusterDatum[],
      simulation: d3.Simulation<NodeDatum, LinkDatum<NodeDatum>>
    ) {
      const hull = svg?.select('g#hulls')
        .selectAll('path')
        .data(clusters)
        .join('path')
        .attr('d', '')
        .attr('stroke', '#ddd')
        .attr('stroke-width', '13px')
        .attr('stroke-linejoin', 'round')
        .attr('stroke-width', 20)
        .style('fill', '#ddd')
        .call(this.drag(simulation))

      hull?.append('title')
        .text(d => d.name)

      return hull
    },

    createClusterNodeSelection (
      svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
      clusters: ClusterDatum[],
      simulation: d3.Simulation<NodeDatum, LinkDatum<NodeDatum>>
    ) {
      return svg?.select('g#clusters') // Add element g (g for group)
        .selectAll('circle') // Select all of type 'circle'
        .data(clusters, d => (d as ClusterDatum).name)
        .join('circle')
        // Set the placement and radius for each node
        .attr('cx', () => { return Math.random() * 300 - 150 }) // Random because, then the simulation can move them around
        .attr('cy', () => { return Math.random() * 300 - 150 })
        .attr('r', 1)
        .style('opacity', 0)
        .call(this.drag(simulation)) // Handle dragging of the nodes
    },

    createRouteLinkSelection (
      svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
      routes: RouteDatum[]
    ) {
      const routeLink = svg?.select('g#routes') // Add element g (g for group)
        .selectAll('line') // Select all of type 'line'
        .data(routes) // Insert the list of links
        .join('line')
        .attr('stroke-opacity', 0.6)
        .attr('stroke', d => d.ntv_error ? '#f00' : '#999') // Set line to red, if it has an error
        .attr('stroke-width', 2)
        .style('opacity', d => d.isSearchMatch ? 1.0 : 0.2)

      routeLink?.append('title') // Set title (hover text) for erronious link
        .text(d => d.ntv_error ? 'Something\'s Wrong' : '')

      return routeLink
    },

    createLeafLinkSelection(
      svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
      leafs: RouteDatum[])
    {
      const leafLink = svg?.select('g#leafs') // Add element g (g for group)
        .selectAll('line') // Select all of type 'line'
        .data(leafs) // Insert the list of links
        .join('line')
        .attr('stroke-opacity', 0.6)
        .attr('stroke', d => d.ntv_error ? '#f00' : '#00f') // Set line to red, if it has an error
        .attr('stroke-width', 2)
        .style('opacity', d => d.isSearchMatch ? 1.0 : 0.2)
        .style ("stroke-dasharray", ("3,3"))
      leafLink?.append('title') // Set title (hover text) for erronious link
        .text(d => d.ntv_error ? 'Something\'s Wrong' : '')
      //TODO: detect how leafs connect (in order to get arrow-direction?)
     return leafLink
    },

    createServerNodeSelection (
      svg: d3.Selection<SVGSVGElement, unknown, HTMLElement, HTMLElement> | null,
      servers: ServerDatum[],
      simulation: d3.Simulation<NodeDatum, LinkDatum<NodeDatum>>
    ) {
      const serverNode = svg?.select('g#servers')
        .selectAll('circle') // Select all of type 'circle'
        .data(servers, d => (d as ServerDatum).server_id)
        .join(
          enter => enter.append('circle'),
          update => update,
          exit => exit.remove()
        )
        .attr('stroke', '#888')
        .attr('stroke-width', 3)
        .attr('cx', () => { return Math.random() }) // Random because, then the simulation can move them around
        .attr('cy', () => { return Math.random() })
        .attr('r', 5)
        .attr('fill', d => d.ntv_error ? '#f00' : '#000') // Make node red if it has error
        .style('opacity', d => d.isSearchMatch ? 1.0 : 0.2)
        .call(this.drag(simulation)) // Handle dragging of the nodes
        .on('click', (d, i) => { // Log the value of the chosen node on click
          this.$emit('node-click')

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
    },

    getHullPath (cluster: ClusterDatum, servers: ServerDatum[]) {
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

      return this.svgPath(hullCoords || nodesCoords) // Polygonhull returns null for 2 or fewer nodes.
    },

    svgPath (coordinates: [number, number][]): string {
      // Create closed SVG path from coordinates
      const initialValue = ''
      const hullPath: string = coordinates?.reduce((str, coords, index, array) =>
        index === 0
          ? `${str} M ${coords[0]},${coords[1]}` // Initially use MoveTo command 'M'
          : `${str} L ${coords[0]},${coords[1]}` // otherwise use LineTo command 'L'
      , initialValue) + ' Z' // End with ClosePath command 'Z'
      return hullPath
    },

    drag (simulation: d3.Simulation<NodeDatum, LinkDatum<NodeDatum>>): d3.DragBehavior<Element, ServerDatum, ServerDatum | SubjectPosition> & ((this: Element, event: any, d: ServerDatum) => void) {
      function dragstarted (event: D3DragEvent<SVGElement, NodeDatum, ServerDatum>, d: ServerDatum) {
        if (!event.active) simulation.alphaTarget(0.3).restart()
        d.fx = d.x
        d.fy = d.y
      }
      function dragged (event: D3DragEvent<SVGElement, NodeDatum, ServerDatum>, d: ServerDatum) {
        d.fx = event.x
        d.fy = event.y
      }
      function dragended (event: D3DragEvent<SVGElement, NodeDatum, Node>, d: ServerDatum) {
        if (!event.active) simulation.alphaTarget(0)
        d.fx = null
        d.fy = null
      }
      return d3.drag<Element, ServerDatum>()
        .on('start', dragstarted)
        .on('drag', dragged)
        .on('end', dragended)
    },

    calculateViewBoxValue (width: number, height: number, viewBoxScalar: number): string {
      const viewBoxTop = -width / 2
      const viewBoxLeft = -height / 2
      const viewBoxRight = width
      const viewBoxBottom = height
      const viewBoxValue = `${viewBoxTop * viewBoxScalar}, ${viewBoxLeft * viewBoxScalar}, ${viewBoxRight * viewBoxScalar}, ${viewBoxBottom * viewBoxScalar}`
      return viewBoxValue
    },
    zoomIn () {
      if (this.zoomLevel < 300) {
        this.$refs.zoom.zoomIn(1.20)
        this.zoomLevel = this.zoomLevel +20
      }
    },
    zoomOut () {
      if (this.zoomLevel > 100) {
        this.$refs.zoom.zoomOut(0.80)
        this.zoomLevel = this.zoomLevel -20
      }
    }
  }
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
#zoomer {
  position: absolute;
  left: 0;
  top: 0;
  z-index: -1;
  width: 100vw;
  height: 100vh;
}
#zoomButtons {
  position: absolute;
  left: 0;
  bottom: 0;
  margin-left: 420px;
  margin-bottom: 17px;
}
.zoomButtons {
  width: 40px;
  margin: 3px;
}
</style>
