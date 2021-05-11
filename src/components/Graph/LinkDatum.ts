import d3, { SimulationNodeDatum } from 'd3'

export default class LinkDatum<T extends SimulationNodeDatum> implements d3.SimulationLinkDatum<T> {
    source!: T;
    target!: T;
    ntv_error!: boolean;
    isSearchMatch = true;
}