using nio2so.Data.Common;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.Datablob.Structures;
using System.Reflection;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    /// <summary>
    /// Marks this method as a handler for a specific <see cref="TSODBRequestWrapper"/> type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolDatabaseHandler : Attribute, ITSOProtocolHandlerAttribute
    {
        public TSOProtocolDatabaseHandler(uint DatabaseAction)
        {
            CLSID = DatabaseAction;
        }

        public UIntEnum CLSID { get; set; }
        uint ITSOProtocolHandlerAttribute.ItemID => (uint)CLSID;
    }
    /// <summary>
    /// Marks this method as a handler for a specific <see cref="ITSODataBlobPDU"/> type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolDatablobHandler : Attribute, ITSOProtocolHandlerAttribute
    {
        public TSOProtocolDatablobHandler(uint CLS_ID)
        {
            CLSID = CLS_ID;
        }

        public UIntEnum CLSID { get; set; }
        uint ITSOProtocolHandlerAttribute.ItemID => (uint)CLSID;
    }

    /// <summary>
    /// A base class for <see cref="ITSOProtocolRegulator"/> objects that provides functionality for mapping 
    /// an incoming PDU type to a method that will handle the incoming PDU.
    /// </summary>
    public abstract class TSOProtocol : TSOProtocolBase
    {
        public delegate void VoltronInvokationDelegate(TSOVoltronPacket PDU);
        public delegate void VoltronDatabaseInvokationDelegate(TSODBRequestWrapper PDU);
        public delegate void VoltronDataBlobInvokationDelegate(ITSODataBlobPDU PDU);

        protected new TSOPreAlphaLoggerService Logger => (TSOPreAlphaLoggerService)Server.Logger;

        protected TSOProtocol() : base(
            TSOProtocolMatchingOption.Create<TSO_PreAlpha_VoltronPacketTypes, TSOVoltronPacket, TSOProtocolHandler, VoltronInvokationDelegate>(),
            TSOProtocolMatchingOption.Create<TSO_PreAlpha_VoltronPacketTypes, TSODBRequestWrapper, TSOProtocolDatabaseHandler, VoltronDatabaseInvokationDelegate>(),
            TSOProtocolMatchingOption.Create<TSO_PreAlpha_VoltronPacketTypes, ITSODataBlobPDU, TSOProtocolDatablobHandler, VoltronDataBlobInvokationDelegate>()
            )
        {

        }

        protected virtual bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            return false;
        }

        public override bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (Server == null) throw new NullReferenceException("No server instance!!!");

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<(uint, TSOVoltronPacket)>());

            void Invoke(Delegate d, object parameter) => MethodInvoker.Create(d.Method).Invoke(this, parameter);

            switch (PDU.KnownPacketType())
            {
                case TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU:
                    {
                        var dbPDU = (TSODBRequestWrapper)PDU;
                        if (GetMapFor<TSODBRequestWrapper>().TryGetValue((uint)dbPDU.TSOSubMsgCLSID, out var action))
                        {
                            Invoke(action, dbPDU);
                            return true;
                        }
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_DATABLOB_PDU:
                case TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU:
                    {
                        var broadcastPDU = (ITSODataBlobPDU)PDU;
                        if (GetMapFor<ITSODataBlobPDU>().TryGetValue((uint)broadcastPDU.SubMsgCLSID, out var action))
                        {
                            Invoke(action, broadcastPDU);
                            return true;
                        }
                        if (OnUnknownDataBlobPDU(broadcastPDU)) return true;
                    }
                    break;
                default:
                    {
                        if (GetMapFor<TSOVoltronPacket>().TryGetValue(PDU.VoltronPacketType, out var action))
                        {
                            Invoke(action, PDU);
                            return true;
                        }
                    }
                    break;
            }
            Response = null;
            return false;
        }
    }
}
