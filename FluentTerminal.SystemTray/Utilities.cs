using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace FluentTerminal.SystemTray
{
    public static class Utilities
    {
        public static int? GetAvailablePort(int startingPort)
        {
            var usedPorts = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            var connections = properties.GetActiveTcpConnections();
            usedPorts.AddRange(connections.Where(c => c.LocalEndPoint.Port >= startingPort).Select(c => c.LocalEndPoint.Port));

            var endPoints = properties.GetActiveTcpListeners();
            usedPorts.AddRange(endPoints.Where(e => e.Port >= startingPort).Select(e => e.Port));

            endPoints = properties.GetActiveUdpListeners();
            usedPorts.AddRange(endPoints.Where(e => e.Port >= startingPort).Select(e => e.Port));

            usedPorts.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
            {
                if (!usedPorts.Contains(i))
                {
                    return i;
                }
            }
            return null;
        }
    }
}
