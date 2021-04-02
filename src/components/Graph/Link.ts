import d3 from 'd3'
import Node from './Node'

export default class Link implements d3.SimulationLinkDatum<Node> {
    ntv_error!: boolean;
    source!: Node;
    target!: Node;
}
