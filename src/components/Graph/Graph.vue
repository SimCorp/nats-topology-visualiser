<template>
  <div id='graph'>
    <div id='visualizer'></div>
  </div>
</template>

<script lang='ts'>
import * as d3 from 'd3'
import axios from 'axios'
import Link from './Link'
import Node from './Node'
import DraggableNode from './DraggableNode'
import { D3DragEvent, SimulationNodeDatum } from 'd3'

function calulateViewBoxValue (width: number, height: number, viewBoxScalar: number) {
  const viewBoxTop = -width / 2
  const viewBoxLeft = -height / 2
  const viewBoxRight = width
  const viewBoxBottom = height
  const viewBoxValue = `${viewBoxTop * viewBoxScalar}, ${viewBoxLeft * viewBoxScalar}, ${viewBoxRight * viewBoxScalar}, ${viewBoxBottom * viewBoxScalar}`
  return viewBoxValue
}

const drag = (simulation: d3.Simulation<Node, Link>) => {
  function dragstarted (event: D3DragEvent<SVGElement, SimulationNodeDatum, Node>, d: DraggableNode) {
    if (!event.active) simulation.alphaTarget(0.3).restart()
    d.fx = d.x
    d.fy = d.y
  }
  function dragged (event: D3DragEvent<SVGElement, SimulationNodeDatum, Node>, d: DraggableNode) {
    d.fx = event.x
    d.fy = event.y
  }
  function dragended (event: D3DragEvent<SVGElement, SimulationNodeDatum, Node>, d: DraggableNode) {
    if (!event.active) simulation.alphaTarget(0)
    d.fx = null
    d.fy = null
  }
  return d3.drag()
    .on('start', dragstarted as unknown as null)
    .on('drag', dragged as unknown as null)
    .on('end', dragended as unknown as null)
}

export default {
  name: 'Graph',
  props: {
  },
  async mounted (): Promise<void> {
    const width = 1000
    const height = 800
    const viewBoxScalar = 0.5

    const varzResponse = await axios.get('https://localhost:5001/nodes')
    const nodes: Node[] = varzResponse.data

    const routezResponse = await axios.get('https://localhost:5001/links')
    const links: Link[] = routezResponse.data

    // Physics for moving the nodes together
    const simulation: d3.Simulation<Node, Link> = d3.forceSimulation(nodes)
      .force('link', d3.forceLink(links).id(d => (d as Node).server_id))
      .force('charge', d3.forceManyBody())
      .force('x', d3.forceX())
      .force('y', d3.forceY())

    // SVG canvas
    const svg = d3.select('#visualizer')
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .attr('viewBox', calulateViewBoxValue(width, height, viewBoxScalar))

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
      .attr('cx', () => { return Math.random() * width }) // Random because, then the simulation can move them arround
      .attr('cy', () => { return Math.random() * height })
      .attr('r', 5)
      .attr('fill', d => d.ntv_error ? '#f00' : '#000') // Make node red if it has error
      .call(() => drag(simulation)) // Handle dragging of the nodes
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

</script>

<style scoped>

</style>
