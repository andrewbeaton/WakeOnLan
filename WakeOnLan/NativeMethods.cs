//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Andrew Beaton">
//     Copyright (c) Andrew Beaton. All rights reserved. 
// </copyright>
//-----------------------------------------------------------------------
namespace WakeOnLan
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class for Platform Invocation native methods.
    /// </summary>
    public class NativeMethods
    {
        /// <summary>
        /// Sends an ARP request using the iphlpapi.dll.
        /// </summary>
        /// <param name="destIP">The destination IP address.</param>
        /// <param name="srcIP">The source IP address.</param>
        /// <param name="macAddr">The destination MAC address.</param>
        /// <param name="phyAddrLen">The physical address length.</param>
        /// <returns>Returns 0 if successful.</returns> 
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        internal static extern int SendARP(int destIP, int srcIP, byte[] macAddr, ref uint phyAddrLen);
    }
}