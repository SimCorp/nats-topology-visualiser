import d3 from 'd3'

export default class NodeDatum implements d3.SimulationNodeDatum {
    ntv_error!: boolean;
    x = 0;
    y = 0;
    fx: number | undefined | null;
    fy: number | undefined | null;
    isSearchMatch = true;
}