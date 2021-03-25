/* eslint-disable @typescript-eslint/camelcase */
export default function processData ([servers, routes]: any[]) {
  const processedServers = servers
  // Patch for a missing node from varz
  // TODO dynamically handle these types of errors
  processedServers.push({
    server_id: 'NCJSE2JXMGGWA2UTKDPEZSY56B6BS3QCMLWE7KDE5MSOQDQA5FBL3TQO'
  })

  interface Link {
    source: string;
    target: string;
  }
  const processedRoutes: Link[] = []
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

  return [processedServers, processedRoutes]
}
