namespace WakeOnLan
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using CommandLine;

    class Program
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

        static void Main(string[] args)
        {
            string ipAddress = null;
            string macAddress = null;
            int port = 9;

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.IPAddress != null)
                {
                    ipAddress = options.IPAddress;
                }

                if (options.MacAddress != null)
                {
                    macAddress = options.MacAddress;
                }

                port = options.Port;
            }

            if (options.Verbose)
            {
                Console.WriteLine("IP Address: {0} MAC Address: {1} Port: {2}", ipAddress, macAddress, port);
            }

            if (ipAddress == null && macAddress == null)
            {
                Console.WriteLine(options.GetUsage());
                return;
            }

            if (ipAddress != null && macAddress == null)
            {
                if (options.Verbose)
                {
                    Console.WriteLine("Trying to determine MAC Address from IP Address.");
                }

                try
                {
                    macAddress = GetMacAddressFromIPAddress(ipAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error getting MAC address from IP address: {0}", ex.Message);
                    return;
                }

                if (macAddress == null)
                {
                    Console.WriteLine("Could not determine mac address.");
                    return;
                }

                if (options.Verbose)
                {
                    Console.WriteLine("MAC address determined to be {0}", macAddress);
                }
            }

            try
            {
                if (options.Verbose)
                {
                    Console.WriteLine("Sending Wake on Lan packet to {0} on UDP port {1}", macAddress, port);
                }

                SendWakeOnLanPacket(macAddress, port);

                Console.WriteLine(String.Format("Sent Wake on Lan packet to {0} on UDP port {1}", macAddress, port));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending Wake on Lan packet: {0}", ex.Message);
                return;
            }
        }

        /// <summary>
        /// Gets the mac address of a machine based on IP address.
        /// Tries a number of methods to obtain this information.
        /// </summary>
        /// <param name="ipAddress">The IP address to look up.</param>
        /// <returns>The mac address if found.</returns>
        private static string GetMacAddressFromIPAddress(string ipAddress)
        {
            string macAddress = null;

            try
            {
                macAddress = GetMacAddressFromARPTable(ipAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (macAddress == null)
            {
                try
                {
                    macAddress = GetMacAddressFromIPHelperAPI(ipAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return macAddress;
        }

        /// <summary>
        /// Sends a Wake-On-Lan packet to the specified MAC address and port.
        /// </summary>
        /// <param name="macAddress">Physical MAC address to receive the WOL packet.</param>
        /// <param name="port">The UDP port to receive the WOL packet.</param>
        private static void SendWakeOnLanPacket(string macAddress, int port)
        {
            var macAddressByteArray = ConvertMacAddressStringToByteArray(macAddress);

            SendWakeOnLanPacket(macAddressByteArray, port);
        }

        /// <summary>
        /// Sends a Wake-On-Lan packet to the specified MAC address and port.
        /// </summary>
        /// <param name="macAddress">Physical MAC address to recieve the WOL packet.</param>
        /// <param name="port">The UDP port to receive the WOL packet.</param>
        private static void SendWakeOnLanPacket(byte[] macAddress, int port)
        {
            // WOL packet is sent over UDP 255.255.255.0:{port}.
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, port);

            byte[] packet = CreateWakOnLanPacket(macAddress);

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }

        /// <summary>
        /// Creates the magic packet required for Wake on Lan requests.
        /// </summary>
        /// <param name="macAddress">The mac address to send to.</param>
        /// <returns>The magic packet.</returns>
        private static byte[] CreateWakOnLanPacket(byte[] macAddress)
        {
            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes
            // sequence containing the MAC address.
            byte[] packet = new byte[17 * 6];

            // Trailer of 6 times 0xFF.
            for (int i = 0; i < 6; i++)
            {
                packet[i] = 0xFF;
            }

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    packet[i * 6 + j] = macAddress[j];
                }
            }

            return packet;
        }

        /// <summary>
        /// Gets a MAC address from an IP address on the local network 
        /// by parsing the ARP table.
        /// </summary>
        /// <param name="ipAddress">The IP address to obtain the MAC addressfor</param>
        /// <returns>The MAC address.</returns>
        private static string GetMacAddressFromARPTable(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentNullException("No IP address provided.");
            }

            string macAddress = string.Empty;

            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();

            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');

            if (substrings.Length < 8)
            {
                throw new Exception("Unknown Mac Address.");
            }

            macAddress = (substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                     + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                     + "-" + substrings[7] + "-"
                     + substrings[8].Substring(0, 2)).ToUpper();

            return macAddress;
        }

        /// <summary>
        /// Gets a MAC address from an IP address on the local network
        /// by using the Microsoft IP Helper API (iphlpapi.dll)
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa366073%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="ipAddress">The IP address to obtain the MAC addressfor</param>
        /// <returns>The MAC address.</returns>
        private static string GetMacAddressFromIPHelperAPI(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentNullException("No IP address provided.");
            }

            byte[] parts = new byte[6];
            uint macAddressLength = (uint)parts.Length;

            var ip = System.BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);

            if (SendARP(ip, 0, parts, ref macAddressLength) != 0)
            {
                throw new Exception("No ARP entries found.");
            }

            string[] substrings = new string[(int)macAddressLength];

            if (substrings.Length != 6)
            {
                throw new Exception("Unknown Mac Address.");
            }

            for (int i = 0; i < macAddressLength; i++)
            {
                substrings[i] = parts[i].ToString("x2");
            }

            var macAddress = string.Join("-", substrings).ToUpper();

            return macAddress;
        }

        /// <summary>
        /// Converts a string containing a MAC address into a byte array.
        /// </summary>
        /// <param name="macAddress">The string mac address.</param>
        /// <returns>A byte array containing the mac address.</returns>
        private static byte[] ConvertMacAddressStringToByteArray(string macAddress)
        {
            string[] macAddressArray = null;

            if (macAddress.Contains("-"))
            {
                macAddressArray = macAddress.Split('-');
            }

            if (macAddress.Contains(":"))
            {
                macAddressArray = macAddress.Split(':');
            }

            byte[] macAddressBytes = new byte[6];

            for (int count = 0; count < macAddressArray.Length; count++)
            {
                macAddressBytes[count] = byte.Parse(macAddressArray[count], System.Globalization.NumberStyles.HexNumber);
            }

            return macAddressBytes;
        }
    }
}