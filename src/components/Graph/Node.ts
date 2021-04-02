import d3 from 'd3'

export default class Node implements d3.SimulationNodeDatum {
    server_id!: string;
    server_name!: string;
    ntv_error!: boolean;
    x = 0;
    y = 0;
}
