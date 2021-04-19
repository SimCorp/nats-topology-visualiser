import ServerDatum from './ServerDatum'

export default class ClusterDatum implements d3.SimulationNodeDatum {
    name!: string;
    servers!: ServerDatum[];
    x = 0;
    y = 0;
    fx: number | undefined | null;
    fy: number | undefined | null;
    isSearchMatch = true;
}