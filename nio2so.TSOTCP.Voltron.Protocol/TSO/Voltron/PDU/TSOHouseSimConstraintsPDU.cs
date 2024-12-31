using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// This needs to be called IN ORDER of receiving. As in, once you receive the <see cref="TSOLoadHouseResponsePDU"/> from
    /// the Client, this should be sent immediately thereafter to reduce client hanging possibility.
    /// <para>This PDU will tell the Client to load the house you specify.</para>
    /// <para>It should match any previous HouseIDs given to the Client, say for example in <see cref="DBWrappers.TSOGetRoommateInfoByLotIDResponse.HouseID"/></para>
    /// <para>Failure to do so may prevent the SimsRegulator from moving into Loading... state and requesting the HouseBlobPDU</para>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU)]
    public class TSOHouseSimConstraintsResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU;
        /// <summary>
        /// Default, parameterless constructor. Default value for <see cref="HouseID"/> is <see cref="TSOVoltronConst.MyHouseID"/>
        /// </summary>
        public TSOHouseSimConstraintsResponsePDU() : base() { MakeBodyFromProperties(); }
        /// <summary>
        /// Makes a new <see cref="TSOHouseSimConstraintsResponsePDU"/> using the specified <paramref name="houseID"/>
        /// </summary>
        /// <param name="houseID">The lot the game should load.</param>
        public TSOHouseSimConstraintsResponsePDU(uint houseID) : base()
        {
            HouseID = houseID;
            MakeBodyFromProperties();
        }

        public uint HouseID { get; set; } = TSOVoltronConst.MyHouseID;
    }
}
