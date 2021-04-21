import NodeDatum from './NodeDatum';

export default class ServerDatum extends NodeDatum {
    server_id!: string;
    server_name!: string;
    ntv_cluster!: string;
}