import LinkDatum from './LinkDatum'
import ClusterDatum from './ClusterDatum'

export default class GatewayDatum extends LinkDatum<ClusterDatum> {
    errors!: string[];
    errorsAsString!: string;
}