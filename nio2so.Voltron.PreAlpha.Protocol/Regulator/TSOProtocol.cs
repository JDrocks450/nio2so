using nio2so.Data.Common;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.Datablob.Structures;

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
    /// For use with The Sims Online Pre-Alpha Voltron Server.<para/>
    /// A base class for <see cref="ITSOProtocolRegulator"/> objects that provides functionality for mapping 
    /// an incoming PDU type to a method that will handle the incoming PDU.
    /// </summary>
    public abstract class TSOProtocol : TSOProtocolBase
    {
        /// <summary>
        /// A <see cref="TSODBRequestWrapper"/> handler delegate.
        /// </summary>
        /// <param name="PDU"></param>
        public delegate void VoltronDatabaseInvokationDelegate(TSODBRequestWrapper PDU);
        /// <summary>
        /// A <see cref="ITSODataBlobPDU"/> handler delegate.
        /// </summary>
        /// <param name="PDU"></param>
        public delegate void VoltronDataBlobInvokationDelegate(ITSODataBlobPDU PDU);

        protected new TSOPreAlphaLoggerService? Logger => base.Logger as TSOPreAlphaLoggerService;

        protected TSOProtocol() : base(
            TSOProtocolMatchingOption.CreateVoltron<TSO_PreAlpha_VoltronPacketTypes>(),
            TSOProtocolMatchingOption.Create<TSO_PreAlpha_VoltronPacketTypes, TSODBRequestWrapper, TSOProtocolDatabaseHandler, VoltronDatabaseInvokationDelegate>(),
            TSOProtocolMatchingOption.Create<TSO_PreAlpha_VoltronPacketTypes, ITSODataBlobPDU, TSOProtocolDatablobHandler, VoltronDataBlobInvokationDelegate>()
            )
        {

        }

        protected virtual bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            return false;
        }

        protected override bool TryHandleSpecialVoltronPDU(TSOVoltronPacket PDU, ref TSOProtocolRegulatorResponse Response)
        {
            // Special VoltronPDU handling for TSO_PreAlpha_Packets
            switch (PDU.KnownPacketType())
            {
                //DB WRAPPERS
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
                //ITSODataBlobPDUs
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
            }
            return false;
        }
    }
}
