using nio2so.TSOTCP.City.TSO.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol
{
    public interface ITSOServer
    {
        public bool IsRunning { get; set; }

        void Slip(ITSOServer cityServer, TSOVoltronPacket PDU);
    }
}
