using MiscUtil.Conversion;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.TSO.Aries;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    /// <summary>
    /// Stores the header information of a <see cref="TSOVoltronPacket"/>
    /// </summary>
    /// <param name="TSOPDUID"> The <see cref="TSO_PreAlpha_VoltronPacketTypes"/> PDU Type </param>
    /// <param name="PDUPayloadSize"> The size of the data, including the header </param>
    internal record TSOVoltronPacketHeader(ushort TSOPDUID, uint PDUPayloadSize)
    {
        public const byte HEADER_SIZE = sizeof(ushort) + sizeof(uint); // <-- Just in case you forget
    }

    internal interface ITSOVoltron
    {
        void MakeBodyFromProperties();
        int ReflectFromBody(Stream BodyStream);
        int ReflectFromBody(byte[] BodyData);
    }

    internal abstract class TSOVoltronPacket : ExtendedBufferOperations, ITSOVoltron
    {
        public virtual string FriendlyPDUName => GetType().Name;
        public abstract UInt16 VoltronPacketType { get; }
        public TSO_PreAlpha_VoltronPacketTypes KnownPacketType => (TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType;
        public UInt32 PayloadSize { get; protected set; }

        /// <summary>
        /// Public-Parameterless constructure to allow the framework to work correctly. 
        /// <para>I HIGHLY recommend you use <see cref="FromAriesPacket{T}(TSOTCPPacket)"/> with a destination type. I worked hard on it.</para>
        /// </summary>
        public TSOVoltronPacket() { }

        /// <summary>
        /// When overridden by a class, can be used to throw exceptions when checking the packet after being received.
        /// </summary>
        public virtual void EnsureNoErrors() { }

        /// <summary>
        /// Uses reflection to create a packet body from the properties you implement. 
        /// <para>You should be using the <see cref="TSOVoltronString"/> attribute to control this feature.</para>
        /// <para>ONLY super-class PUBLIC PROPERTIES are included! No properties from 
        /// <see cref="TSOVoltronPacket"/> or <see cref="ExtendedBufferOperations"/> will be included.</para>
        /// <para>Even if your packet has no properties, you need to call this before it can be
        /// sent to the Server as this writes the Voltron header to the packet body.</para>
        /// </summary>
        public virtual void MakeBodyFromProperties()
        {
            AllocateBody(2);  //clear old data...!         

            if (VoltronPacketType == 0x00)
                throw new InvalidDataException($"{nameof(VoltronPacketType)} is 0x00! This cannot be -- no packet should ever leave without a type.");

            EmplaceBody(EndianBitConverter.Big.GetBytes((UInt16)VoltronPacketType)); // voltron packet type emplaced
            EmplaceBody(EndianBitConverter.Big.GetBytes((UInt32)00)); // followed by placeholder data as we have no clue how big it is. 

            foreach (var property in GetPropertiesToCopy()) {                
                bool hasAttrib = getPropertyAttribute(property, out TSOVoltronValueTypes type) != default;
                object? myValue = property.GetValue(this);
                if (myValue == default) continue;
                var endianConverter = (EndianBitConverter)(
                        type == TSOVoltronValueTypes.BigEndian ?
                            EndianBitConverter.Big :
                            EndianBitConverter.Little);

                bool wroteValue = true;

                if (property.PropertyType == typeof(Byte[]))
                {
                    EmplaceBody((byte[])myValue);
                    continue;
                }

                //---NUMERICS
                if (property.PropertyType.IsAssignableTo(typeof(UInt16)))
                    EmplaceBody(endianConverter.GetBytes((UInt16)myValue));
                else if (property.PropertyType == typeof(Int16))
                    EmplaceBody(endianConverter.GetBytes((Int16)myValue));
                else if (property.PropertyType.IsAssignableTo(typeof(UInt32)))
                    EmplaceBody(endianConverter.GetBytes((UInt32)myValue));
                else if (property.PropertyType == typeof(Int32))
                    EmplaceBody(endianConverter.GetBytes((Int32)myValue));
                else if (property.PropertyType == typeof(byte))
                    EmplaceBody((byte)myValue);
                else if (property.PropertyType == typeof(DateTime))
                    EmplaceBody((uint)((DateTime)myValue).Minute); // probably not right
                else wroteValue = false;

                if (wroteValue) continue;
                if (!hasAttrib) // AUTOSELECT
                    type = TSOVoltronValueTypes.Pascal;

                //---STRINGS
                void WriteString(string Text)
                {
                    if (type == TSOVoltronValueTypes.Pascal)
                    {
                        EmplaceBody(0x80, 0x00);
                        EmplaceBody(EndianBitConverter.Big.GetBytes((UInt16)Text.Length));
                    }
                    else Text += '\0';
                    EmplaceBody(Encoding.UTF8.GetBytes(Text));
                    wroteValue = true;
                }

                if (property.PropertyType == typeof(string[]))
                {
                    foreach (var str in (string[])myValue)
                        WriteString(str);
                }
                else if (myValue is String myStringValue)
                    WriteString(myStringValue);

                if (wroteValue) continue;
                throw new Exception("VOLTRON PDU -- When writing, I couldn't find a matching type!");
            }

            ReevaluateSize();
        }

        public void ReevaluateSize()
        {
            SetPosition(2);
            PayloadSize = BodyLength; // set my payload size
            EmplaceBody(EndianBitConverter.Big.GetBytes((UInt32)PayloadSize)); // put it into the packet body at the placeholder
        }
        /// <summary>
        /// Reads the first <see cref="TSOVoltronPacketHeader.HEADER_SIZE"/> bytes of the <paramref name="bodyStream"/>
        /// to to determine the Type and Size of the packet.
        /// </summary>
        /// <param name="bodyStream"></param>
        /// <param name="fooVoltronPacketType"></param>
        /// <param name="fooPayloadSize"></param>
        public static void ReadVoltronHeader(Stream bodyStream, out ushort fooVoltronPacketType, out uint fooPayloadSize)
        {
            long initialPosition = bodyStream.Position;
            byte[] headerBytes = new byte[6];
            bodyStream.ReadExactly(headerBytes, 0, 6);
            ReadVoltronHeader(headerBytes, out fooVoltronPacketType, out fooPayloadSize);
            bodyStream.Position = initialPosition;
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
        public static void ReadVoltronHeader(byte[] Data, out UInt16 VoltronPacketType, out UInt32 VPacketSize)
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

            foreach (var property in GetPropertiesToCopy()) {                
                //check if this property is the body array property
                if (property.GetCustomAttribute<TSOVoltronBodyArray>() != default)
                { // it is.
                    if (property.PropertyType != typeof(Byte[]))
                        throw new CustomAttributeFormatException("You applied the TSOVoltronBodyArray attribute to ... not a byte[]! Are you testing me?");
                    int remainingData = (int)(PayloadSize - BodyPosition);
                    byte[] destBuffer = new byte[remainingData];
                    _bodyBuffer.Read(destBuffer, 0, remainingData);
                    property.SetValue(this, destBuffer);
                    break; // halt execution
                }

                var attribute = getPropertyAttribute(property, out TSOVoltronValueTypes type);
                bool hasAttrib = attribute != default;
                //BigEndian by default!
                Endianness dataEndianMode = type == TSOVoltronValueTypes.BigEndian ? Endianness.BigEndian : Endianness.LittleEndian;
                bool readValue = true;                

                //---NUMBERS
                if (property.PropertyType == typeof(byte))
                {
                    property.SetValue(this, (byte)ReadBodyByte());
                    continue;
                }
                else if (property.PropertyType == typeof(UInt16) || property.PropertyType == typeof(Int16))
                {
                    ushort fromPacket = ReadBodyUshort(); // read an unsigned short
                    if (property.PropertyType == typeof(UInt16)) // is it even an unsigned short?
                        property.SetValue(this, fromPacket); // yeah
                    else property.SetValue(this, Convert.ToInt16(fromPacket)); // no it wasn't, convert it. uhh, i think this works?
                    continue;
                }
                else if (property.PropertyType == typeof(UInt32) || property.PropertyType == typeof(Int32))
                {
                    uint fromPacket = ReadBodyDword(); // read an unsigned int
                    if (property.PropertyType == typeof(UInt32)) // is it even an unsigned int?
                        property.SetValue(this, fromPacket); // yeah
                    else property.SetValue(this, Convert.ToInt32(fromPacket)); // no it wasn't, convert it. uhh, i think this works?
                    continue;
                }
                else readValue = false;

                if (readValue) continue;

                if (property.PropertyType == typeof(DateTime))
                {
                    uint fromPacket = ReadBodyDword();
                    var value = DateTime.UnixEpoch.AddSeconds(fromPacket);
                    property.SetValue(this, value);
                    continue;
                }

                if (!hasAttrib) // AUTOSELECT
                    type = TSOVoltronValueTypes.Pascal;

                //---STRING
                string destValue = "Error.";
                switch (type)
                {
                    case TSOVoltronValueTypes.Pascal:
                        {
                            ushort strHeader = ReadBodyUshort(Endianness.LittleEndian);
                            if (strHeader != 0x80)
                                throw new Exception("This is supposed to be a string but I don't think it is one...");
                            ushort len = ReadBodyUshort(Endianness.BigEndian);
                            byte[] strBytes = ReadBodyByteArray((int)len);
                            destValue = Encoding.UTF8.GetString(strBytes);
                        }
                        break;
                    case TSOVoltronValueTypes.NullTerminated:
                        destValue = ReadBodyNullTerminatedString(attribute.NullTerminatedMaxLength);
                        break;
                    case TSOVoltronValueTypes.Length_Prefixed_Byte:
                        {
                            int len = ReadBodyByte();
                            byte[] strBytes = ReadBodyByteArray((int)len);
                            destValue = Encoding.UTF8.GetString(strBytes);
                        }
                        break;
                }
                    
                property.SetValue(this, destValue);
            }

            return (int)PayloadSize;
        }

        private TSOVoltronString? getPropertyAttribute(PropertyInfo property, out TSOVoltronValueTypes ValueType)
        {
            var attribute = property.GetCustomAttribute<TSOVoltronString>();
            TSOVoltronValueTypes type = TSOVoltronValueTypes.BigEndian;
            int pascalLength = 0;
            if (attribute == null)
            {
                if (property.PropertyType == typeof(string))
                    type = TSOVoltronValueTypes.Pascal;
            }
            else
                type = attribute.Type;
            ValueType = type;
            return attribute;
        }

        /// <summary>
        /// Calls <see cref="TSOVoltronPacket.MakeBodyFromProperties"/> once more then wraps this packet into a 
        /// <see cref="TSOTCPPacket"/> for tranmission to the remote endpoint
        /// </summary>
        /// <param name="VoltronPacket"></param>
        /// <returns></returns>
        public static TSOTCPPacket MakeVoltronAriesPacket(TSOVoltronPacket VoltronPacket)
        {
            //Call this once more to ensure all properties are properly saved before being sent
            //to the remote party
            VoltronPacket.MakeBodyFromProperties();
            return new TSOTCPPacket(TSOAriesPacketTypes.Voltron, 0, VoltronPacket.Body);
        }
        /// <summary>
        /// Calls <see cref="TSOVoltronPacket.MakeBodyFromProperties"/> once more then wraps this packet into a 
        /// <see cref="TSOTCPPacket"/> for tranmission to the remote endpoint
        /// </summary>
        public static TSOTCPPacket MakeVoltronAriesPacket(params TSOVoltronPacket[] VoltronPackets) => MakeVoltronAriesPacket(VoltronPackets);
        /// <summary>
        /// Calls <see cref="TSOVoltronPacket.MakeBodyFromProperties"/> once more then wraps these packets into a 
        /// <see cref="TSOTCPPacket"/> for tranmission to the remote endpoint
        /// </summary>
        public static TSOTCPPacket MakeVoltronAriesPacket(IEnumerable<TSOVoltronPacket> VoltronPackets)
        {
            long totalSize = VoltronPackets.Sum(x => x.PayloadSize) + 2;
            byte[] bodyBuffer = new byte[totalSize];
            int currentIndex = 0;

            foreach (var p in VoltronPackets)
            {
                //Call this once more to ensure all properties are properly saved before being sent
                //to the remote party
                p.MakeBodyFromProperties();

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
                Packet = TSOVoltronPacket.ParsePacket<T>(Data, out ReadAmount);
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
            System.IO.Directory.CreateDirectory(Directory);
            string myName = $"{(Incoming ? "IN" : "OUT")} [{KnownPacketType}] PDU {DateTime.Now.ToFileTime()}.dat";
            File.WriteAllBytes(Path.Combine(Directory, myName), Body);
        }
    }
}
