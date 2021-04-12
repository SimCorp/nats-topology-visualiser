import d3 from 'd3'

export default class ServerDatum implements d3.SimulationNodeDatum {
    server_id!: string;
    server_name!: string;
    ntv_error!: boolean;
    x = 0;
    y = 0;
    fx: number | undefined | null;
    fy: number | undefined | null;
}