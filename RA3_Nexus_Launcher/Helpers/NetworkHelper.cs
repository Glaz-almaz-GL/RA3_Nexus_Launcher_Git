using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class NetworkHelper
    {
        /// <summary>
        /// Возвращает список всех активных IPv4-адресов, назначенных сетевым интерфейсам на локальной машине.
        /// </summary>
        /// <returns>Список строковых представлений IPv4-адресов.</returns>
        public static List<string> GetAllIPv4Addresses()
        {
            try
            {
                List<string> ipv4Addresses = [.. NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(addrInfo => addrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                       !IPAddress.IsLoopback(addrInfo.Address))
                    .Select(addrInfo => addrInfo.Address.ToString())];

                return ipv4Addresses;
            }
            catch (NetworkInformationException ex)
            {
                // Обработка ошибок, связанных с получением информации о сетевых интерфейсах
                Console.WriteLine($"Ошибка получения сетевых интерфейсов: {ex.Message}");
                return [];
            }
        }
    }
}
