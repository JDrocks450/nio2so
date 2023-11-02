using MiscUtil.Conversion;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    internal interface ITSOVoltron
    {
        void MakeBodyFromProperties();
        int ReflectFromBody(Stream BodyStream);
        int ReflectFromBody(byte[] BodyData);
    }

    internal abstract class TSOVoltronPacket : ExtendedBufferOperations, ITSOVoltron
    {
        public virtual string FriendlyPDUName => GetType().Name;
        public virtual UInt16 VoltronPacketType { get; } = 0x0;
        public TSO_PreAlpha_VoltronPacketTypes KnownPacketType => (TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType;
        public UInt32 PayloadSize { get; protected set; }

        /// <summary>
        /// Public-Parameterless constructure to allow the framework to work correctly. 
        /// <para>I HIGHLY recommend you use <see cref="FromAriesPacket{T}(TSOTCPPacket)"/> with a destination type. I worked hard on it.</para>
        /// </summary>
        public TSOVoltronPacket() { }

        /// <summary>
        /// Uses reflection to create a packet body from the properties you implement. 
        /// <para>You should be using the <see cref="TSOVoltronString"/> attribute to control this feature.</para>
        /// <para>ONLY super-class PUBLIC PROPERTIES are included! No properties from 
        /// <see cref="TSOVoltronPacket"/> or <see cref="ExtendedBufferOperations"/> will be included.</para>
        /// <para>Even if your packet has no properties, you need to call this before it can be
        /// sent to the Server as this writes the Voltron header to the packet body.</para>
        /// </summary>
        public void MakeBodyFromProperties()
        {
            AllocateBody(2);  //clear old data...!         

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

                //---NUMERICS
                if (property.PropertyType == typeof(UInt16))
                    EmplaceBody(endianConverter.GetBytes((UInt16)myValue));
                else if (property.PropertyType == typeof(Int16))
                    EmplaceBody(endianConverter.GetBytes((Int16)myValue));
                else if (property.PropertyType == typeof(UInt32))
                    EmplaceBody(endianConverter.GetBytes((UInt32)myValue));
                else if (property.PropertyType == typeof(Int32))
                    EmplaceBody(endianConverter.GetBytes((Int32)myValue));
                else if (property.PropertyType == typeof(byte))
                    EmplaceBody((byte)myValue);
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
                }

                if (property.PropertyType == typeof(string[]))
                {
                    foreach (var str in (string[])myValue)
                        WriteString(str);
                }
                else if (myValue is String myStringValue)
                    WriteString(myStringValue);
            }

            ReevaluateSize();
        }

        public void ReevaluateSize()
        {
            SetPosition(2);
            PayloadSize = BodyLength; // set my payload size
            EmplaceBody(EndianBitConverter.Big.GetBytes((UInt32)PayloadSize)); // put it into the packet body at the placeholder
        }

        public static void ReadVoltronHeader(Stream bodyStream, out ushort fooVoltronPacketType, out uint fooPayloadSize)
        {
            long initialPosition = bodyStream.Position;
            byte[] headerBytes = new byte[6];
            bodyStream.ReadExactly(headerBytes, 0, 6);
            ReadVoltronHeader(headerBytes, out fooVoltronPacketType, out fooPayloadSize);
            bodyStream.Position = initialPosition;
        }
        public static void ReadVoltronHeader(byte[] Data, out UInt16 VoltronPacketType, out UInt32 VPacketSize)
        {
            VoltronPacketType = EndianBitConverter.Big.ToUInt16(Data, 0);
            VPacketSize = EndianBitConverter.Big.ToUInt32(Data, 2);
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
        public int ReflectFromBody(Stream BodyStream)
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
                if (type == TSOVoltronValueTypes.Pascal)
                {
                    ushort strHeader = ReadBodyUshort(Endianness.LittleEndian);
                    if (strHeader != 0x80)
                        throw new Exception("This is supposed to be a string but I don't think it is one...");
                    ushort len = ReadBodyUshort(Endianness.BigEndian);
                    byte[] strBytes = ReadBodyByteArray((int)len);
                    destValue = Encoding.UTF8.GetString(strBytes);
                }
                else
                    destValue = ReadBodyNullTerminatedString(attribute.NullTerminatedMaxLength);
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

        public static TSOTCPPacket MakeVoltronAriesPacket(TSOVoltronPacket VoltronPacket, bool includePacketCloser = true)
        {
            if (includePacketCloser)
            {
                //emplace packet closer
                VoltronPacket.SetPosition((int)VoltronPacket.BodyLength);
                VoltronPacket.EmplaceBody(new byte[] { 0x41, 0x01 });
                VoltronPacket.ReevaluateSize();
            }
            return new TSOTCPPacket(TSOAriesPacketTypes.Voltron, 0, VoltronPacket.Body);
        }      
            
        public static TSOTCPPacket MakeVoltronAriesPacket(params TSOVoltronPacket[] VoltronPackets) => MakeVoltronAriesPacket(VoltronPackets);            
        public static TSOTCPPacket MakeVoltronAriesPacket(IEnumerable<TSOVoltronPacket> VoltronPackets)
        {
            long totalSize = VoltronPackets.Sum(x => x.PayloadSize) + 2;
            byte[] bodyBuffer = new byte[totalSize];
            int currentIndex = 0;

            //put packet closer in last packet 
            var lastVoltronPacket = VoltronPackets.Last();
            lastVoltronPacket.SetPosition((int)lastVoltronPacket.BodyLength);
            lastVoltronPacket.EmplaceBody(new byte[] { 0x41, 0x01 });
            lastVoltronPacket.ReevaluateSize();

            foreach (var p in VoltronPackets)
            {
                p.Body.CopyTo(bodyBuffer, currentIndex);
                currentIndex += (int)p.BodyLength;
            }
            return new TSOTCPPacket(TSOAriesPacketTypes.Voltron, 0, bodyBuffer);            
        }

        public static IEnumerable<TSOVoltronPacket> ParseAllPackets(TSOTCPPacket AriesPacket)
        {
            List<TSOVoltronPacket> packets = new();
            AriesPacket.SetPosition(0);
            uint currentIndex = 0;
            do
            {
                TSOVoltronPacket? cTSOVoltronpacket = default;
                try
                {                    
                    ReadVoltronHeader(AriesPacket.BodyStream, out ushort VPacketType, out uint Size);
                    currentIndex += Size;
                    cTSOVoltronpacket = TSOPDUFactory.CreatePacketObjectByPacketType((TSO_PreAlpha_VoltronPacketTypes)VPacketType);                    
                    byte[] temporaryBuffer = new byte[Size];
                    AriesPacket.BodyStream.ReadExactly(temporaryBuffer, 0, (int)Size);
                    if (cTSOVoltronpacket is TSOBlankPDU)
                    {
                        TSOPDUFactory.LogDiscoveryPacketToDisk(VPacketType, temporaryBuffer);
                        continue;
                    }
                    cTSOVoltronpacket.ReflectFromBody(temporaryBuffer);
                }
                catch (Exception ex)
                {
                    QConsole.WriteLine("TSOVoltronPacket_Warnings", $"An error occured in the ParsePackets function. {ex.Message}");
                    AriesPacket.SetPosition((int)currentIndex);
                    continue;
                }
                if (cTSOVoltronpacket != default)
                    packets.Add(cTSOVoltronpacket);
            }
            while (!AriesPacket.IsBodyEOF);
            return packets;
        }

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
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<PropertyInfo> GetPropertiesToCopy() =>
            GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
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
                sb.AppendLine($"    {property.Name}: {property.GetValue(this)}");            
            return $"c{FriendlyPDUName} -> {{\n{sb}\n}}";
        }
    }
}
