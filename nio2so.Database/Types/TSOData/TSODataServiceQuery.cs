using nio2so.DataService.Common.Tokens;
using nio2so.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Types.TSOData
{
    public interface ITSODataObjectQuery
    {
        string StructID { get; set; }
        string ObjectID { get; set; }

        public uint GetDataDefinitionStructID();
        public uint GetObjectID();
    }

    public interface ITSODataObjectFieldQuery : ITSODataObjectQuery
    {
        public string FieldID { get; set; }
        public uint GetFieldID();
    }

    /// <summary>
    /// Class schema for a TSO Data Service query for use with the TSO Data Service Controller in the nio2so Data Service.
    /// <para/>It is built around the following versions:
    /// <list type="bullet"/><inheritdoc cref="TSOVersions.TSO_NI_1_1097_1_0"/>
    /// </summary>
    [Serializable]
    public class TSODataServiceQuery : ITSODataObjectQuery, ITSODataObjectFieldQuery
    {
        public string StructID { get; set; } = "0";
        public string ObjectID { get; set; } = "0";
        public string FieldID { get; set; } = "0";

        uint convertBase(string value) => Convert.ToUInt32(value);
        public uint GetDataDefinitionStructID() => convertBase(StructID);
        public uint GetObjectID() => convertBase(ObjectID);
        public uint GetFieldID() => convertBase(ObjectID);

        public static ITSODataObjectQuery MakeObjectQuery(uint StructID, uint ObjectID) => new TSODataServiceQuery()
        {
            ObjectID = ObjectID.ToString(),
            StructID = StructID.ToString()
        };
        public static ITSODataObjectFieldQuery MakeObjectFieldQuery(uint StructID, uint FieldID, uint ObjectID)
        {
            TSODataServiceQuery query = (TSODataServiceQuery)MakeObjectQuery(StructID, ObjectID);
            query.FieldID = FieldID.ToString();
            return query;
        }
    }
}
