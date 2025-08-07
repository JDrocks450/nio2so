using nio2so.TSOTCP.Voltron.Protocol.TSO.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;
using System.Reflection;
using System.Text;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.Datablob.Structures
{
    /// <summary>
    /// Denotes that this <see cref="TSOVoltronPacket"/> has a <see cref="DataBlobContentObject"/> property
    /// that is used in the <see cref="TSOTransmitDataBlobPacket"/> and <see cref="TSOBroadcastDatablobPacket"/>
    /// </summary>
    public interface ITSODataBlobPDU
    {
        TSOPlayerInfoStruct SenderInfo { get; }
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
    public sealed class TSOGenericDataBlobContent
    {
        private TSO_PreAlpha_MasterConstantsTable? _contentCLSID = default;
        public void SetCLSID(TSO_PreAlpha_MasterConstantsTable CLSID) => _contentCLSID = CLSID;

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
        public void SetTo<T>(T ContentObject) where T : ITSODataBlobContentObject => ContentBytes = TSOVoltronSerializer.Serialize(ContentObject);
        /// <summary>
        /// Attempts to map the CLSID stored in this <see cref="TSOGenericDataBlobContent"/> object to a defined type
        /// using the <see cref="TSOVoltronDatablobContent"/> attribute.
        /// <para/>You can store a CLSID to this instance using <see cref="SetCLSID(TSO_PreAlpha_MasterConstantsTable)"/>
        /// </summary>
        /// <param name="CLSID"></param>
        /// <param name="Object"></param>
        /// <returns></returns>
        public bool TryGetByCLSID(TSO_PreAlpha_MasterConstantsTable CLSID, out ITSODataBlobContentObject? Object)
        {
            Object = default;
            foreach (var type in typeof(TSOBroadcastDatablobPacket).Assembly.
                GetTypes().Where(x => x.GetCustomAttribute<TSOVoltronDatablobContent>() != null))
            {
                TSOVoltronDatablobContent attribute = type.GetCustomAttribute<TSOVoltronDatablobContent>();
                if (attribute.Type == CLSID)
                    Object = (ITSODataBlobContentObject)TSOVoltronSerializer.Deserialize(ContentBytes, type);
            }
            return Object != default;
        }
        public override string ToString()
        {
            if (_contentCLSID.HasValue)
            {
                if (TryGetByCLSID(_contentCLSID.Value, out ITSODataBlobContentObject? Object))
                {
                    StringBuilder paramsString = new();
                    paramsString.Append($"({nameof(ITSODataBlobContentObject)}){Object.GetType().Name}(");
                    foreach (var property in Object.GetType().GetProperties())
                        paramsString.Append($"{property.Name}: {property.GetValue(Object)}, ");
                    paramsString.Append(")");
                    return paramsString.ToString();
                }
            }
            return base.ToString();
        }
    }
}
