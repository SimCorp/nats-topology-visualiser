import d3 from 'd3'
import ServerDatum from './ServerDatum'

export default class ClusterDatum implements d3.SimulationNodeDatum {
    name!: string;
    servers!: ServerDatum[];
    x = 0;
    y = 0;
}