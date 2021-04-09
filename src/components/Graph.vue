<template>
  <div id='graph'>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'

function calulateViewBoxValue (width: number, height: number, viewBoxScalar: number) {
  const viewBoxTop = -width / 2
  const viewBoxLeft = -height / 2
  const viewBoxRight = width
  const viewBoxBottom = height
  const viewBoxValue = `${viewBoxTop * viewBoxScalar}, ${viewBoxLeft * viewBoxScalar}, ${viewBoxRight * viewBoxScalar}, ${viewBoxBottom * viewBoxScalar}`
  return viewBoxValue
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

    const varzResponse = await axios.get('https://localhost:5001/nodes')
    const processedServers = varzResponse.data

    const routezResponse = await axios.get('https://localhost:5001/links')
    const processedRoutes = routezResponse.data

    const clusterzResponse = await axios.get('https://localhost:5001/clusters')
    const processedClusters = clusterzResponse.data

    // d3 canvas
    const nodes = processedServers.map(d => Object.create(d))
    const links = processedRoutes.map(d => Object.create(d))
    const clusters = processedClusters.map(d => Object.create(d))

    // Physics for moving the nodes together
    const simulation = d3.forceSimulation(nodes)
      .force('link', d3.forceLink(links).id(d => d.server_id))
      .force('charge', d3.forceManyBody())
      .force('x', d3.forceX())
      .force('y', d3.forceY())

    // SVG canvas
    const svg = d3.select('#visualizer')
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .attr('viewBox', calulateViewBoxValue(width, height, viewBoxScalar))

    // A convex hull (enclosing path) for clusters
    const hull = svg.append('g')
      .selectAll('path')
      .data(clusters)
      .enter()
      .append('path')
      .attr('d', '')
      .attr('opacity', 0.7)
      .attr('stroke', '#ddd')
      .attr('stroke-width', '13px')
      .attr('stroke-linejoin', 'round')
      .style('fill', '#ddd')

    const hullTitles = hull.append('title')
      .text(d => d.name)

    // Connections between nodes
    const link = svg.append('g') // Add element g (g for group)
      .attr('stroke-opacity', 0.6)
      .selectAll('line') // Select all of type 'line'
      .data(links) // Insert the list of links
      .join('line')
      .attr('stroke', d => d.ntv_error ? '#f00' : '#999') // Set line to red, if it has an error
      .attr('stroke-width', 2)

    link.append('title') // Set title (hover text) for erronious link
      .text(d => d.ntv_error ? 'Something\'s Wrong' : '')

    // The nodes
    const node = svg.append('g') // Add element g (g for group)
      .attr('stroke', '#888')
      .attr('stroke-width', 3)
      .selectAll('circle') // Select all of type 'circle'
      .data(nodes)
      .join('circle')
      // Set the placement and radius for each node
      .attr('cx', () => { return Math.random() }) // Random because, then the simulation can move them arround
      .attr('cy', () => { return Math.random() })
      .attr('r', 5)
      .attr('fill', d => d.ntv_error ? '#f00' : '#000') // Make node red if it has error
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

    simulation.on('tick', () => { // What it does whenever the canvas updates
      link
        .attr('x1', d => d.source.x)
        .attr('y1', d => d.source.y)
        .attr('x2', d => d.target.x)
        .attr('y2', d => d.target.y)

      node
        .attr('cx', d => d.x)
        .attr('cy', d => d.y)

      // Set svg path attribute, that encloses nodes
      hull
        .attr('d', d => {
          // TODO: Find a more efficient method (pre process groupings of nodes according to clusters)
          const nodesOfCluster = d.servers.map(serverDatum => {
            return nodes.filter(serverNode => (serverDatum.server_id === serverNode.server_id))
          })
          // Create SVG path from coordinates
          const nodesCoords = nodesOfCluster.map(node => {
            return [node[0].x, node[0].y]
          })
          const hullCoords: [number, number][] = d3.polygonHull(nodesCoords) ? d3.polygonHull(nodesCoords) : nodesCoords
          return svgPath(hullCoords || [])
        })
    })
  }
}

</script>

<style scoped>

</style>
