using nio2so.Formats.TSOData;
using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;

namespace nio2so.Voltron.NewImproved.Protocol.Types.DataService
{
    internal record TSODataServiceProperty(TSODataField FieldInfo, ITSOValue Value);
}
