namespace nio2so.TSOTCP.Voltron.Protocol
{
    public static class HSBSession
    {
        public static bool HSB_Activated = false;
        public static ITSOServer CityServer { get; set; }
        public static ITSOServer RoomServer { get; set; }
    }
}
