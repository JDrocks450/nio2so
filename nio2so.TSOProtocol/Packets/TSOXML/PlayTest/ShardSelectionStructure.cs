using nio2so.TSOHTTPS.Protocol.Data.Credential;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML
{
    public record struct ShardSelectionStructure([property:TSOXMLElementName("Connection-Address")] string ConnectionAddress,
                                                 [property: TSOXMLElementName("Authorization-Ticket")] TSOSessionTicket AuthorizationTicket,
                                                 string PlayerID,
                                                 uint ConnectionID,
                                                 int EntitlementLevel,
                                                 uint AvatarID) : ITSOXMLStructure;
}
