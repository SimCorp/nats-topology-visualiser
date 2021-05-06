import NodeDatum from './NodeDatum';
import ServerDatum from './ServerDatum'

export default class ClusterDatum extends NodeDatum {
    name!: string;
    servers!: ServerDatum[];
}