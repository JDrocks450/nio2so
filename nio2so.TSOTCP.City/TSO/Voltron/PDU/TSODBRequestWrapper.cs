using MiscUtil.Conversion;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU)]
    internal class TSODBRequestWrapper : TSOVoltronPacket
    {
        public TSODBRequestWrapper() : base() 
        { }
        public TSODBRequestWrapper(string ariesID,
                                   string masterID,
                                   ushort bitfield_Arg1,
                                   uint messageSize,
                                   uint tSOPacketFormatCLSID,
                                   byte flags,
                                   uint transactionID,                                   
                                   uint tSOSubMsgCLSID,
                                   byte[] body) : this()
        {
            AriesID = ariesID;
            MasterID = masterID;
            Bitfield_Arg1 = bitfield_Arg1;
            MessageSize = messageSize;
            TSOPacketFormatCLSID = tSOPacketFormatCLSID;
            TransactionID = transactionID;
            Flags = flags;
            TSOSubMsgCLSID = tSOSubMsgCLSID;
            DBMessageBody = body;
            FillPacketToAvailableSpace();
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU;

        [TSOVoltronString]
        public string AriesID { get; set; }
        [TSOVoltronString]
        public string MasterID { get; set; }
        public ushort Bitfield_Arg1 { get; set; }
        /// <summary>
        /// The distance (in bytes) from the end of the DWORD to the end of the packet. 
        /// <para>Basically, all other fields after this one are included in the "Body" of the packet.</para>
        /// <para>For clarity and usability, values stored in the Body have been pulled-up to the class level,
        /// such as <see cref="TSOPacketFormatCLSID"/></para>
        /// </summary>
        public uint MessageSize { get; set; }
        /// <summary>
        /// TSO has different classes in the library that correspond with the structure of these requests.
        /// <para>This is the identifier for which class should be created to house the data.</para>
        /// </summary>
        public uint TSOPacketFormatCLSID { get; set; }
        public byte Flags { get; set; }
        public uint TransactionID { get; set; }        
        /// <summary>
        /// Beneath the overall packet type there is a CLSID for the individual request being made.
        /// </summary>
        public uint TSOSubMsgCLSID { get; set; }
        [TSOVoltronBodyArray]
        public byte[] DBMessageBody { get; set; }

        /// <summary>
        /// Use this function to extend the Body property to the <see cref="MessageSize"/> parameter.
        /// <para>See: <see cref="MessageSize"/> property for more info.</para>
        /// </summary>
        protected void FillPacketToAvailableSpace()
        {
            //TSOPacketFormat + Flags + TransID + SubMSgCLSID + CurrentBodySize
            int currentSize = sizeof(uint) + sizeof(byte) + sizeof(uint) + sizeof(uint) + DBMessageBody.Length;
            int delta = (int)(MessageSize - currentSize); // size diff
            if (delta <= 0) return; // yikes, not necessary to even do this.
            int newSize = delta + DBMessageBody.Length;
            var fooBody = DBMessageBody;
            Array.Resize(ref fooBody, newSize);
            fooBody.TSOFillArray((uint)newSize);
            DBMessageBody = fooBody;
            currentSize = sizeof(uint) + sizeof(byte) + sizeof(uint) + sizeof(uint) + DBMessageBody.Length;
            if (currentSize != MessageSize)
                throw new Exception("This should never happen.");
        }

        /* Implement these later
        public bool HasData1 => (Flags & (1 << 1)) != 0;
        public bool HasData2 => (Flags & (1 << 2)) != 0;
        public bool HasData3 => (Flags & (1 << 3)) != 0;
        public bool HasExtraCLSID => (Flags & (1 << 5)) != 0;
        */

    }
}
