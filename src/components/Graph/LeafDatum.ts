import LinkDatum from './LinkDatum'
import ServerDatum from './ServerDatum'

export default class LeafDatum extends LinkDatum<ServerDatum> {
    connections!: [{
        account: string,
        ip: string,
        port: number,
        in_msgs: number,
        out_msgs: number,
        in_bytes: number,
        out_bytes: number,
        subscriptions: number
    }]
}