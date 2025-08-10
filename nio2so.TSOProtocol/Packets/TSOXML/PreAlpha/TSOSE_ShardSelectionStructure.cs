using nio2so.TSOHTTPS.Protocol.Data.Credential;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha
{
    public record struct TSOSE_ShardSelectionStructure([property:TSOXMLElementName("Connection-Address")] string ConnectionAddress,
                                                 [property: TSOXMLElementName("Authorization-Ticket")] TSOSessionTicket AuthorizationTicket,
                                                 uint PlayerID) : ITSOXMLStructure;
}
