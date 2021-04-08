import d3 from 'd3'
import ServerDatum from './ServerDatum'

export default class RouteDatum implements d3.SimulationLinkDatum<ServerDatum> {
    ntv_error!: boolean;
    source!: ServerDatum;
    target!: ServerDatum;
}