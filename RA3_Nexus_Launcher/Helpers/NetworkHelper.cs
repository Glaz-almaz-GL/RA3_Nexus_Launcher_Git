using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RA3_Nexus_Launcher.Helpers
{
    public readonly record struct NetworkInterfaceInfo(string IpAddress, string InterfaceName);

    public static class NetworkHelper
    {
        /// <summary>
        /// Возвращает список всех активных IPv4-адресов, назначенных сетевым интерфейсам на локальной машине.
        /// </summary>
        /// <returns>Список строковых представлений IPv4-адресов.</returns>
        public static List<NetworkInterfaceInfo> GetNetworkInterfaceInfo()
        {
            try
            {
                var interfacesInfo = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses
                        .Where(addrInfo => addrInfo.Address.AddressFamily == AddressFamily.InterNetwork &&
                                          !IPAddress.IsLoopback(addrInfo.Address))
                        .Select(addrInfo => new NetworkInterfaceInfo(addrInfo.Address.ToString(), ni.Name)));

                return [.. interfacesInfo]; // Создаём List из IEnumerable
            }
            catch (NetworkInformationException ex)
            {
                NotificationHelpers.ShowError("Error getting network interfaces", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
                return [];
            }
        }
    }
}
