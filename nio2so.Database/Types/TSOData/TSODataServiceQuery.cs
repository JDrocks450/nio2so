using nio2so.DataService.Common.Tokens;
using nio2so.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Types.TSOData
{
    /// <summary>
    /// Class schema for a TSO Data Service query for use with the TSO Data Service Controller in the nio2so Data Service.
    /// <para/>It is built around the following versions:
    /// <list type="bullet"/><inheritdoc cref="TSOVersions.TSO_NI_1_1097_1_0"/>
    /// </summary>
    [Serializable]
    public class TSODataServiceQuery
    {
        public string StructID { get; set; } = "0";
        public string ObjectID { get; set; } = "0";

        uint convertBase(string value) => Convert.ToUInt32(value);
        public uint GetDataDefinitionStructID() => convertBase(StructID);
        public uint GetObjectID() => convertBase(ObjectID);
    }
}
