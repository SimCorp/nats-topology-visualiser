/* eslint-disable @typescript-eslint/camelcase */

export default function processData ({ servers, routes }: DataInput) {
  const processedServers = servers

  const idToServer = new Map()
  processedServers.forEach(server => {
    idToServer.set(server.server_id, server)
  })

  const processedRoutes: Route[] = []
  routes.forEach((server: any) => {
    const source: string = server.server_id
    server.routes.forEach((route: any) => {
      const target: string = route.remote_id
      processedRoutes.push({
        source: source,
        target: target
      })
    })
  })

  // Data integrety validation
  processedServers.forEach(server => {
    server.ntv_error = false
  })

  const missingServerIds = new Set<string>()
  const erroniousRoutes = new Set()
  processedRoutes.forEach(route => {
    if (!idToServer.has(route.source)) {
      missingServerIds.add(route.source)
      erroniousRoutes.add(route)
    }
    if (!idToServer.has(route.target)) {
      missingServerIds.add(route.target)
      erroniousRoutes.add(route)
    }
  })

  // Patch for a missing node from varz
  // TODO dynamically handle these types of errors
  missingServerIds.forEach(serverId => {
    processedServers.push({
      server_id: serverId,
      ntv_error: true
    })
  })

  erroniousRoutes.forEach((route: any) => {
    route.ntv_error = true
  })

  return {
    processedServers,
    processedRoutes
  }
}

interface DataInput {
  servers: any[];
  routes: any[];
}
interface Route {
  source: string;
  target: string;
}
interface DataOutput {
  processedServers: any[];
  processedRoutes: Route[];
}
