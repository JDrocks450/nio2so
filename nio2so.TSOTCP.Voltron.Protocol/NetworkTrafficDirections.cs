#define MAKEMANY
#undef MAKEMANY

namespace nio2so.TSOTCP.City.TSO
{
    /// <summary>
    /// Describes the flow of traffic for a network event
    /// </summary>
    public enum NetworkTrafficDirections
    {
        /// <summary>
        /// Default. None set.
        /// </summary>
        NONE,
        /// <summary>
        /// Traffic coming in from a remote connection
        /// </summary>
        INBOUND,
        /// <summary>
        /// Traffic going to a remote connection
        /// </summary>
        OUTBOUND,
        /// <summary>
        /// Traffic going to another recipient on the local machine
        /// </summary>
        LOCALMACHINE,
        /// <summary>
        /// A communication that has been created but not yet sent
        /// </summary>
        CREATED
    }
}
