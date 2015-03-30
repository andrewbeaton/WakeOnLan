//-----------------------------------------------------------------------
// <copyright file="CommandOptions.cs" company="Andrew Beaton">
//     Copyright (c) Andrew Beaton. All rights reserved. 
// </copyright>
//-----------------------------------------------------------------------
namespace WakeOnLan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommandLine;

    /// <summary>
    /// Configuration of the command line options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the destination IP address.
        /// </summary>
        [Option('i', "ip", HelpText = "The destination IP Address to send the Wake on Lan packet.")]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the destination MAC address.
        /// </summary>
        [Option('m', "mac", HelpText = "The destination MAC Address to send the Wake on Lan packet.")]
        public string MacAddress { get; set; }

        /// <summary>
        /// Gets or sets the destination UDP port number.
        /// </summary>
        [Option('p', "port", DefaultValue = 9, HelpText = "The UDP port to recieve the Wake on Lan packet.")]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable verbose output.
        /// </summary>
        [Option('v', "verbose", HelpText = "Print verbose details during execution.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets the command line help text.
        /// </summary>
        /// <returns>The command line help text.</returns>
        [HelpOption]
        public string GetUsage()
        { 
            var usage = new StringBuilder();
            usage.AppendLine("Wake on Lan 1.0");
            usage.AppendLine("Sends a Wake on Lan packet to a specified IP address or MAC address.");
            usage.AppendLine(string.Empty);
            usage.AppendLine("Usage:");
            usage.AppendLine("WakeOnLan.exe [-i IP Address] [-m MAC Address] [-p UDP port]");
            usage.AppendLine(string.Empty);
            usage.AppendLine("-i -ip         The destination IP address.");
            usage.AppendLine("-m -mac        The destination MAC address. (Format: 00-00-00-00-00-00)");
            usage.AppendLine("-p -port       The destination UDP port. (Default: 9)");
            usage.AppendLine("-v -verbose    Print verbose details during execution.");
            usage.AppendLine(string.Empty);
            usage.AppendLine("Examples:");
            usage.AppendLine("WakeOnLan.exe -i 192.168.1.1 -p 9");
            usage.AppendLine("WakeOnLan.exe -m 00-0c-29-14-98-f3"); 
            
            return usage.ToString();
        }
    }
} 
