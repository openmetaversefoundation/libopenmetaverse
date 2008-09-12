using System;
using System.Collections.Generic;
using System.Net;

namespace OpenMetaverse
{
    // this class encapsulates a single packet that
    // is either sent or received by a UDP socket
    public class UDPPacketBuffer
    {
        /// <summary>Size of the byte array used to store raw packet data</summary>
        public const int BUFFER_SIZE = 4096;
        /// <summary>Size of the temporary buffer for zerodecoding and 
        /// zeroencoding this packet</summary>
        public const int ZERO_BUFFER_SIZE = 4096;
        /// <summary>Raw packet data buffer</summary>
        public readonly byte[] Data;
        /// <summary>Temporary buffer used for zerodecoding and zeroencoding
        /// this packet</summary>
        public readonly byte[] ZeroData;
        /// <summary>Length of the data to transmit</summary>
        public int DataLength;
        /// <summary>EndPoint of the remote host</summary>
        public EndPoint RemoteEndPoint;

        /// <summary>
        /// Create an allocated UDP packet buffer for receiving a packet
        /// </summary>
        public UDPPacketBuffer()
        {
            Data = new byte[UDPPacketBuffer.BUFFER_SIZE];
            ZeroData = new byte[UDPPacketBuffer.ZERO_BUFFER_SIZE];
            // Will be modified later by BeginReceiveFrom()
            RemoteEndPoint = (EndPoint)new IPEndPoint(Settings.BIND_ADDR, 0);
        }

        /// <summary>
        /// Create an allocated UDP packet buffer for sending a packet
        /// </summary>
        /// <param name="endPoint">EndPoint of the remote host</param>
        public UDPPacketBuffer(IPEndPoint endPoint)
        {
            Data = new byte[UDPPacketBuffer.BUFFER_SIZE];
            ZeroData = new byte[UDPPacketBuffer.ZERO_BUFFER_SIZE];
            RemoteEndPoint = (EndPoint)endPoint;
        }
    }

    /// <summary>
    /// Object pool for packet buffers. This is used to allocate memory for all
    /// incoming and outgoing packets, and zerocoding buffers for those packets
    /// </summary>
    public class PacketBufferPool : ObjectPoolBase<UDPPacketBuffer>
    {
        private IPEndPoint EndPoint;

        /// <summary>
        /// Initialize the object pool in client mode
        /// </summary>
        /// <param name="endPoint">Server to connect to</param>
        /// <param name="itemsPerSegment"></param>
        /// <param name="minSegments"></param>
        public PacketBufferPool(IPEndPoint endPoint, int itemsPerSegment, int minSegments)
            : base()
        {
            EndPoint = endPoint;
            Initialize(itemsPerSegment, minSegments, true, 1000 * 60 * 5);
        }

        /// <summary>
        /// Initialize the object pool in server mode
        /// </summary>
        /// <param name="itemsPerSegment"></param>
        /// <param name="minSegments"></param>
        public PacketBufferPool(int itemsPerSegment, int minSegments)
            : base()
        {
            EndPoint = null;
            Initialize(itemsPerSegment, minSegments, true, 1000 * 60 * 5);
        }

        /// <summary>
        /// Returns a packet buffer with EndPoint set if the buffer is in
        /// client mode, or with EndPoint set to null in server mode
        /// </summary>
        /// <returns>Initialized UDPPacketBuffer object</returns>
        protected override UDPPacketBuffer GetObjectInstance()
        {
            if (EndPoint != null)
                // Client mode
                return new UDPPacketBuffer(EndPoint);
            else
                // Server mode
                return new UDPPacketBuffer();
        }
    }

    public static class Pool
    {
        public static PacketBufferPool PoolInstance;

        /// <summary>
        /// Default constructor
        /// </summary>
        static Pool()
        {
            PoolInstance = new PacketBufferPool(new IPEndPoint(Settings.BIND_ADDR, 0), 16, 1);
        }

        /// <summary>
        /// Check a packet buffer out of the pool
        /// </summary>
        /// <returns>A packet buffer object</returns>
        public static WrappedObject<UDPPacketBuffer> CheckOut()
        {
            return PoolInstance.CheckOut();
        }
    }
}
