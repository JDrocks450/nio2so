using nio2so.TSOTCP.City.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob.Structures
{
    /// <summary>
    /// Denotes that this <see cref="TSOVoltronPacket"/> has a <see cref="DataBlobContentObject"/> property
    /// that is used in the <see cref="TSOTransmitDataBlobPacket"/> and <see cref="TSOBroadcastDatablobPacket"/>
    /// </summary>
    public interface ITSODataBlobPDU
    {
        TSO_PreAlpha_MasterConstantsTable SubMsgCLSID { get; set; }
        TSOGenericDataBlobContent DataBlobContentObject { get; set; }
    }
    /// <summary>
    /// Denotes the class is a <see cref="ITSODataBlobContentObject"/> -- as in it can be used the 
    /// content to a <see cref="ITSODataBlobPDU"/>
    /// </summary>
    public interface ITSODataBlobContentObject
    {

    }

    /// <summary>
    /// The content of a <see cref="TSOBroadcastDatablobPacket"/> or <see cref="TSOTransmitDataBlobPacket"/>
    /// 
    /// <para/><i>How it works:</i>
    /// <para/>This will deserialize initially as simply a byte[] containing the payload of the data past the 
    /// CLSID.
    /// Using the CLSID from the packet this is found in
    /// will define the structure of the data, which can then be used to fully decode the data.
    /// <para/>To decode the data, you can use the <see cref="GetAs{T}"/> function, and if you want to set data to
    /// this object you should use <see cref="SetTo{T}(T)"/>
    /// </summary>
    public sealed class TSOGenericDataBlobContent {
        [TSOVoltronBodyArray] public byte[] ContentBytes { get; set; } = new byte[0];

        public TSOGenericDataBlobContent() { }
        public TSOGenericDataBlobContent(byte[] contentBytes) : this()
        {
            ContentBytes = contentBytes;
        }

        public TSOGenericDataBlobContent(ITSODataBlobContentObject? content) : this()
        {
            if (content != null) SetTo(content);
        }
        /// <summary>
        /// Deserializes the <see cref="ContentBytes"/> to the given type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAs<T>() where T : ITSODataBlobContentObject, new() => TSOVoltronSerializer.Deserialize<T>(ContentBytes);
        /// <summary>
        /// Sets the <see cref="ContentBytes"/> property to the Serialized <paramref name="ContentObject"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ContentObject"></param>
        public void SetTo<T>(T ContentObject) where T : ITSODataBlobContentObject => ContentBytes = TSOVoltronSerializer.Serialize<T>(ContentObject);
    }
}
