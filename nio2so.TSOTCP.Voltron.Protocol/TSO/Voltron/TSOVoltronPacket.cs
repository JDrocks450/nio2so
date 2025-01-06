using MiscUtil.Conversion;
using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Aries;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron
{
    /// <summary>
    /// Stores the header information of a <see cref="TSOVoltronPacket"/>
    /// </summary>
    /// <param name="TSOPDUID"> The <see cref="TSO_PreAlpha_VoltronPacketTypes"/> PDU Type </param>
    /// <param name="PDUPayloadSize"> The size of the data, including the header </param>
    public record TSOVoltronPacketHeader(ushort TSOPDUID, uint PDUPayloadSize)
    {
        public const byte HEADER_SIZE = sizeof(ushort) + sizeof(uint); // <-- Just in case you forget
    }

    public interface ITSOVoltron
    {
        void MakeBodyFromProperties();
        int ReflectFromBody(Stream BodyStream);
        int ReflectFromBody(byte[] BodyData);
    }

    public abstract class TSOVoltronPacket : ExtendedBufferOperations, ITSOVoltron
    {
        public virtual string FriendlyPDUName => GetType().Name;
        public abstract ushort VoltronPacketType { get; }
#if DEBUG
        /// <summary>
        /// DEBUG! Used when you want to manually change the <see cref="VoltronPacketType"/>
        /// <para/>Note: This will not ship in Release builds and only works for Serialization.
        /// </summary>
        public ushort? Debug_OverrideMyVoltronPacketType { get; set; } = null;
#endif
        public TSO_PreAlpha_VoltronPacketTypes KnownPacketType => (TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType;
        public uint PayloadSize { get; protected set; }

        /// <summary>
        /// Public-Parameterless constructure to allow the framework to work correctly. 
        /// <para>I HIGHLY recommend you use <see cref="FromAriesPacket{T}(TSOTCPPacket)"/> with a destination type. I worked hard on it.</para>
        /// </summary>
        public TSOVoltronPacket() { }

        /// <summary>
        /// When overridden by a class, can be used to throw exceptions when checking the packet after being received.
        /// </summary>
        public virtual void EnsureNoErrors() { }

        public TSOVoltronSerializerGraphItem? MySerializedGraph { get; private set; }

        /// <summary>
        /// Uses reflection to create a packet body from the properties you implement. 
        /// <para>You can use the <see cref="TSOVoltronString"/> and <see cref="TSOVoltronValue"/> attributes to get granular with this feature.</para>
        /// <para>ONLY super-class PUBLIC PROPERTIES are included! No properties from 
        /// <see cref="TSOVoltronPacket"/> or <see cref="ExtendedBufferOperations"/> will be included.</para>
        /// <para/>For <see cref="TSODBRequestWrapper"/> packets, all public properties you wish to include in the packet should be marked with 
        /// <see cref="TSOVoltronDBWrapperField"/> and any you wish to omit should be adorned with <see cref="TSOVoltronIgnorable"/>
        /// <para>Even if your packet has no properties, you need to call this before it can be
        /// sent to the Server as this writes the Voltron header to the packet body.</para>
        /// </summary>
        public virtual void MakeBodyFromProperties()
        {
            AllocateBody(2);  //clear old data...!         

            ushort voltronPacketType = VoltronPacketType;
#if DEBUG
            if (Debug_OverrideMyVoltronPacketType.HasValue)
                voltronPacketType = Debug_OverrideMyVoltronPacketType.Value;
#endif
            if (VoltronPacketType == 0x00)
                throw new InvalidDataException($"{nameof(voltronPacketType)} is 0x00! This cannot be -- no packet should ever leave without a type.");

            EmplaceBody(EndianBitConverter.Big.GetBytes(voltronPacketType)); // voltron packet type emplaced
            EmplaceBody(EndianBitConverter.Big.GetBytes((uint)00)); // followed by placeholder data as we have no clue how big it is. 

            //**Distance to end property attribute map is here**
            //Index, PropertyInfo
            Dictionary<uint, PropertyInfo> distanceToEnds = new();

            MySerializedGraph = new("", GetType(), this, ToShortString());

            foreach (var property in GetPropertiesToCopy())
            {
                if (property.GetCustomAttribute<TSOVoltronDistanceToEnd>() != default)
                {
                    distanceToEnds.Add((uint)BodyPosition, property);
                    EmplaceBody(new byte[sizeof(uint)]); // fill with blank for now
                    continue;
                }
                if (!EmbedProperty(property))
                    throw new Exception($"VOLTRON PDU -- Cannot serialize {property.PropertyType}! Property: {property}");
                MySerializedGraph.Add(TSOVoltronSerializerCore.GetLastGraph());
            }
            //Calculate size from index of the field to the end of the file plus size of property
            foreach (var distanceToEnd in distanceToEnds)
            {
                uint Size = BodyLength - distanceToEnd.Key;
                Size -= sizeof(uint);
                distanceToEnd.Value.SetValue(this, Size);
                SetPosition((int)distanceToEnd.Key);
                EmbedProperty(distanceToEnd.Value);
            }

            ReevaluateSize();
        }

        private bool EmbedProperty(PropertyInfo property) => TSOVoltronSerializerCore.WriteProperty(_bodyBuffer, property, this);

        public void ReevaluateSize()
        {
            SetPosition(2);
            PayloadSize = BodyLength; // set my payload size
            EmplaceBody(EndianBitConverter.Big.GetBytes(PayloadSize)); // put it into the packet body at the placeholder
        }
        /// <summary>
        /// Reads the first <see cref="TSOVoltronPacketHeader.HEADER_SIZE"/> bytes of the <paramref name="bodyStream"/>
        /// to to determine the Type and Size of the packet.
        /// </summary>
        /// <param name="bodyStream"></param>
        /// <param name="fooVoltronPacketType"></param>
        /// <param name="fooPayloadSize"></param>
        public static int ReadVoltronHeader(Stream bodyStream, out ushort fooVoltronPacketType, out uint fooPayloadSize)
        {
            long initialPosition = bodyStream.Position;
            byte[] headerBytes = new byte[6];
            bodyStream.ReadExactly(headerBytes, 0, 6);
            ReadVoltronHeader(headerBytes, out fooVoltronPacketType, out fooPayloadSize);
            bodyStream.Position = initialPosition;
            return 6;
        }
        /// <summary>
        /// Reads the first <see cref="TSOVoltronPacketHeader.HEADER_SIZE"/> bytes of the <paramref name="bodyStream"/>
        /// to to determine the Type and Size of the packet.
        /// </summary>
        public static TSOVoltronPacketHeader ReadVoltronHeader(Stream bodyStream)
        {
            ReadVoltronHeader(bodyStream, out ushort type, out uint size);
            return new(type, size);
        }
        /// <summary>
        /// Reads the first <see cref="TSOVoltronPacketHeader.HEADER_SIZE"/> bytes of the <paramref name="Data"/>
        /// to to determine the Type and Size of the packet.
        /// </summary>
        public static void ReadVoltronHeader(byte[] Data, out ushort VoltronPacketType, out uint VPacketSize)
        {
            VoltronPacketType = EndianBitConverter.Big.ToUInt16(Data, 0);
            VPacketSize = EndianBitConverter.Big.ToUInt32(Data, 2);
        }
        /// <summary>
        /// Reads the first <see cref="TSOVoltronPacketHeader.HEADER_SIZE"/> bytes of the <paramref name="Data"/>
        /// to to determine the Type and Size of the packet.
        /// </summary>
        public static TSOVoltronPacketHeader ReadVoltronHeader(byte[] Data)
        {
            ReadVoltronHeader(Data, out ushort type, out uint size);
            return new(type, size);
        }
        /// <summary>
        /// Reads body data formatted as a The Sims Online Pre-Alpha Voltron packet into this object's destination type. 
        /// <para>Returns the amount of bytes from the array it read.</para>
        /// <para>For split packets, you will need to call this function multiple times -- it will only read one packet at a time.</para>
        /// </summary>
        /// <param name="BodyData"></param>
        public int ReflectFromBody(byte[] BodyData)
        {
            using (var ms = new MemoryStream(BodyData)) // make managed stream
                return ReflectFromBody(ms);
        }
        /// <summary>
        /// Reads a <see cref="Stream"/> containing a The Sims Online Pre-Alpha Voltron packet into this object's destination type.
        /// <para/>This function will not set <paramref name="BodyStream"/> <see cref="Stream.Position"/> 
        /// property to zero, so the packet will be read from the current position onward.
        /// <para>Returns the amount of bytes from the array it read.</para>
        /// <para>For split packets, you will need to call this function multiple times -- it will only read one packet at a time.</para>
        /// </summary>
        /// <param name="BodyStream">A Stream containing a The Sims Online Pre Alpha Voltron packet (header included)</param>
        public virtual int ReflectFromBody(Stream BodyStream)
        {
            ReadVoltronHeader(BodyStream, out ushort fooVoltronPacketType, out uint fooPayloadSize);

            if (VoltronPacketType != fooVoltronPacketType) // DOES IT MATCH THIS OBJECT?
                throw new InvalidDataException($"Developer: The TSOVoltronPacket object you're calling the ReflectFromBody function on is: " +
                    $"{KnownPacketType}, yet the data we got is for: {(TSO_PreAlpha_VoltronPacketTypes)fooVoltronPacketType}!");
            PayloadSize = fooPayloadSize; // PACKET SIZE
            if (PayloadSize > BodyStream.Length) // NOT ENOUGH DATA YET!
                throw new InternalBufferOverflowException("This packet is larger than the supplied data buffer. Get all the data first before calling.");

            _bodyBuffer.SetLength(PayloadSize);
            BodyStream.Position = _bodyBuffer.Position = 0;
            BodyStream.CopyTo(_bodyBuffer, (int)PayloadSize);
            SetPosition(6);
            if (BodyPosition == PayloadSize) return (int)PayloadSize;

            foreach (var property in GetPropertiesToCopy())
            {
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null) continue;
                //**serializable property
                if (!TSOVoltronSerializerCore.ReflectProperty(_bodyBuffer, property, this))
                    throw new ArgumentException($"Could not deserialize: {property}");
                if (IsBodyEOF) break;
            }
            return (int)PayloadSize;
        }

        /// <summary>
        /// Wraps this packet into a <see cref="TSOTCPPacket"/> for tranmission to the remote endpoint
        /// <para/> You must ensure <see cref="MakeBodyFromProperties"/> was called as needed to ensure your
        /// packet is ready to leave.
        /// </summary>
        /// <param name="VoltronPacket"></param>
        /// <returns></returns>
        public static TSOTCPPacket MakeVoltronAriesPacket(TSOVoltronPacket VoltronPacket)
        {
            return new TSOTCPPacket(TSOAriesPacketTypes.Voltron, 0, VoltronPacket.Body);
        }
        /// <summary>
        /// Wraps this packet into a <see cref="TSOTCPPacket"/> for tranmission to the remote endpoint
        /// <para/> You must ensure <see cref="MakeBodyFromProperties"/> was called as needed to ensure your
        /// packet is ready to leave.
        /// </summary>
        public static TSOTCPPacket MakeVoltronAriesPacket(params TSOVoltronPacket[] VoltronPackets) => MakeVoltronAriesPacket(VoltronPackets);
        /// <summary>
        /// Wraps this packet into a <see cref="TSOTCPPacket"/> for tranmission to the remote endpoint
        /// <para/> You must ensure <see cref="MakeBodyFromProperties"/> was called as needed to ensure your
        /// packet is ready to leave.
        /// </summary>
        public static TSOTCPPacket MakeVoltronAriesPacket(IEnumerable<TSOVoltronPacket> VoltronPackets)
        {
            long totalSize = VoltronPackets.Sum(x => x.PayloadSize) + 2;
            byte[] bodyBuffer = new byte[totalSize];
            int currentIndex = 0;

            foreach (var p in VoltronPackets)
            {
                p.Body.CopyTo(bodyBuffer, currentIndex);
                currentIndex += (int)p.BodyLength;
            }
            return new TSOTCPPacket(TSOAriesPacketTypes.Voltron, 0, bodyBuffer);
        }

        public static IEnumerable<TSOVoltronPacket> ParseAllPackets(TSOTCPPacket AriesPacket) =>
            TSOPDUFactory.CreatePacketObjectsFromAriesPacket(AriesPacket);

        public static T? ParsePacket<T>(byte[] Data, out int ReadAmount) where T : ITSOVoltron, new()
        {
            T myT = new T();
            ReadAmount = myT.ReflectFromBody(Data);
            return myT;
        }

        public static bool TryParsePacket<T>(byte[] Data, out T? Packet, out int ReadAmount) where T : ITSOVoltron, new()
        {
            ReadAmount = -1;
            Packet = default;
            try
            {
                Packet = ParsePacket<T>(Data, out ReadAmount);
            }
            catch (Exception ex)
            {
                QConsole.WriteLine("TSOVoltronPackets_Warnings", $"An error occurred when parsing a packet. {ex.Message}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Gets all the properties which are eligible for encoding into the packet body
        /// <para/>Eligible Properties are ones that meet the following conditions:
        /// <list type="bullet">Properties that are NOT named <c>VoltronPacketType</c> or <c>FriendlyPDUName</c></list>
        /// <list type="bullet">Properties enclosed in a <see cref="TSOVoltronPacket"/> or any inheritors (super class down to base class order)</list>
        /// <list type="bullet">Properties that are not <see cref="TSOVoltronIgnorable"/> or any inheritors of this attribute
        /// (for example: <see cref="TSOVoltronDBWrapperField"/> which is also ignorable)</list>
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<PropertyInfo> GetPropertiesToCopy() =>
            GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(
                //Hard-Coded absolutely avoided names
                x => x.Name != "VoltronPacketType" && x.Name != "FriendlyPDUName" &&
                //Filter out all low-level properties that should not be encoded
                x.DeclaringType != typeof(TSOVoltronPacket) && x.DeclaringType != typeof(ExtendedBufferOperations) &&
                //Filter out any properties declared with TSOVoltronIgnorable
                x.GetCustomAttribute<TSOVoltronIgnorable>() == default
            );

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var property in GetPropertiesToCopy())
                sb.Append($"{property.Name}: {property.GetValue(this)}, ");
            string text = sb.ToString();
            if (text.Length > 1)
                text = text.Remove(text.Length - 2);
            return $"{FriendlyPDUName}({text})";
        }

        public virtual string ToShortString(string Arguments = "")
        {
            return ToString();
        }

        /// <summary>
        /// Writes this <see cref="TSOTCPPacket"/>'s contents to the disk
        /// <para>You can use the <paramref name="PacketName"/> parameter to change what Packet Type appears in the brackets in the filename.</para>
        /// By <see langword="default"/>, this would be the <see cref="TSOAriesPacketTypes"/> <see cref="PacketType"/> name of this <see cref="TSOTCPPacket"/>
        /// </summary>
        /// <param name="Incoming">Dictates whether this <see cref="TSOTCPPacket"/> coming from the Server or from the local machine. 
        /// <see langword="true"/> for local machine.</param>
        /// <param name="PacketName">You can use the <paramref name="PacketName"/> parameter to change what Packet Type appears in the brackets in the filename.
        /// By <see langword="default"/>, this would be the <see cref="TSOAriesPacketTypes"/> <see cref="PacketType"/> name of this <see cref="TSOTCPPacket"/></param>
        /// <param name="Directory"></param>
        public void WritePDUToDisk(bool Incoming = true, string Directory = TSOVoltronConst.VoltronPacketDirectory)
        {
            if (!TestingConstraints.LogPackets) return;
            if (!TestingConstraints.LogVoltronPackets) return;
            System.IO.Directory.CreateDirectory(Directory);
            var now = DateTime.Now;
            string myName = $"{(Incoming ? "IN" : "OUT")} [{FriendlyPDUName}] VoltronPDU {now.Hour % 12},{now.Minute},{now.Second},{now.Nanosecond}.dat";
            myName = myName.Replace(':', '=');
            File.WriteAllBytes(Path.Combine(Directory, myName), Body);
        }
    }
}
