using nio2so.Formats.TSOData;
using nio2so.Voltron.Core;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using nio2so.Voltron.NewImproved.Protocol.Types.DataService;
using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator
{

    /// <summary>
    /// Handles all incoming DataService requests from the TSOClient
    /// </summary>
    [TSORegulator]
    public class TSONIDataServiceProtocol : TSOProtocol
    {
        byte[] packet_data = {
		            /* Aries header */
		            0x00,0x00,0x00,0x00,
                    0xA1,0xA2,0xA3,0xA4,
                    0x37,0x01,0x00,0x00, /* hex: 40+50+6c+3b= 137 */

		            /* MyAvatar.Avatar_Name packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x40,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC,
                    0x00,0x00,0x00,0x2E,
                    0x09,0x73,0x60,0x27,
                    0x0F,0xE8,0x80,0x3B,
                    0xA9,0x73,0x60,0xC5,
                    0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x03,
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0x4F,0xFE,0x27,0x0C, // MyAvatar_Avatar_Name
                    0x89,0x6D,0x16,0x88, // string
                    0x08,0x4A,0x6F,0x6C,0x6C,0x79,0x53,0x69,0x6D, /* "JollySim" */
		            0x00,

		            /* MyAvatar.Avatar_Description packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x50,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC, //MyAvatar
                    0x00,0x00,0x00,0x3E, // data length
                    0x09,0x73,0x60,0x27, // cTSOTopicUpdateMessage
                    0x0F,0xE8,0x80,0x3B, // update counter
                    0xA9,0x73,0x60,0xC5, // GZMSGID_cITSOApprovedTopicUpdate
                    0x00,0x00,0x00,0x00, // status code?
                    0x00,0x00,0x00,0x03, // items in this vector
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0x54,0xB9,0x9C,0x9A, // avatar description
                    0x89,0x6D,0x16,0x88, // string
                    0x18,0x42,0x65,0x61,0x72,0x2D,0x79,0x20,0x6e,0x69,0x63,0x65,0x20,0x74,0x6f,0x20,
                    0x6d,0x65,0x65,0x74,0x20,0x79,0x6f,0x75,0x21, /* "Bear-y nice to meet you!" */
		            0x00,

		            /* MyAvatar.Avatar_Appearance packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x6C,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC, // myAvatar
                    0x00,0x00,0x00,0x5A,
                    0x09,0x73,0x60,0x27,
                    0x0F,0xE8,0x80,0x3B,
                    0xA9,0x73,0x60,0xC5,
                    0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x03, // items in this vector
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0xF1,0xB9,0x2C,0x99, // Variable: AvatarAppearance Avatar_Appearance
                    0xA9,0x6E,0x7E,0x5B, // cTSOValue_Property
                    0x89,0x73,0x9A,0x79, // cTSOPropertyObject
                    0x3B,0x04,0x30,0xBF, // AvatarAppearance (type)
                    0x00,0x00,0x00,0x03,
                    0x1D,0x53,0x02,0x75, // BODYOUTFITID
                    0x69,0xD3,0xE3,0xDB, // uint64
                    0x00,0x00,0x01,0x60,0x00,0x00,0x00,0x0D, /* body outfit */
		            0x25,0x11,0xB2,0x9C, // SKIN TONE
                    0xC9,0x76,0x08,0x7C, // byte
                    0x00,
                    0x68,0x74,0x75,0xBF, // HEADID
                    0x69,0xD3,0xE3,0xDB, // uint64
                    0x00,0x00,0x04,0x0F,0x00,0x00,0x00,0x0D, /* head outfit */
		            0x00,

		            /* MyAvatar.Avatar_LotGridXY packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x3B,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC, // MyAvatar
                    0x00,0x00,0x00,0x29,
                    0x09,0x73,0x60,0x27,
                    0x0F,0xE8,0x80,0x3B,
                    0xA9,0x73,0x60,0xC5,
                    0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x03,
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0x7E,0x0D,0xD3,0x5C, // Avatar_LotGridXY
                    0x69,0x6D,0x11,0x89, // uint32
                    0x00,0xB8,0x00,0xD5,
                    0x00
            };

        public override void ServerReady(ITSOServer Server)
        {
            base.ServerReady(Server);

            if (!TryGetService<TSODataDefinitionService>(out TSODataDefinitionService? service))
                throw new InvalidOperationException("Failed to get TSODataDefinitionService.");

            if (!service.Ready)
                throw new InvalidOperationException($"The {nameof(TSODataDefinitionService)} did not load correctly. The TSODataService cannot continue.");
        }

        const uint Avatar_DerivedID = 0x05600332; // ID of the Derived Struct Avatar

        [TSOProtocolHandler((ushort)TSO_NewImproved_VoltronPacketTypes.DataServiceWrapperPDU)]
        public void DATA_SERVICE_REQUEST(TSOVoltronPacket PDU)
        {
            if (PDU is not TSODataServiceWrapperPDU Query)
                throw new InvalidOperationException("Invalid PDU type for DATA_SERVICE_REQUEST handler.");

            var stdMsg = Query.Message.GetMessageAs<TSONetMessageStandard>();

            if (Query.DefinitionStringID != 0xA46E47DC) return;            

            uint MyAvatarID = 0x0539;

            //TESTING FOR LATER
            bool success = TSOData_TryGetObjectInstance(Query.DefinitionStringID, out TSODataServiceRuntimeObject? StructObject);

            dynamic obj = StructObject.GetObjectAsDynamic();

            obj.Avatar_Name = "Testing";
            obj.Avatar_Description = "Completed.";

            RespondToWithObject(Query, StructObject, MyAvatarID);

            return;

            TSOTopicUpdateMessage avatarAppearance = new(new TSOValue_TSODataStructProperty(
                new TSOValue_TSODataStructPropertyWrapper(
                    /*AvatarAppearance (Struct ID)*/ 0x3B0430BF,
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.BodySuitID*/0x1D530275, new TSOValue_Uint64(0x000001600000000D)),
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.SkinTone*/0x2511B29C, new TSOValue_Uint8(0x00)),
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.HeadSuitID*/0x687475BF, new TSOValue_Uint64(0x0000040F0000000D))
                    )), /*MyAvatar.Avatar_Appearance (which is STRUCT type)*/ 0xF1B92C99, Avatar_DerivedID, 0x00000539);
            RespondWith(new TSODataServiceWrapperPDU(Query.SendingAvatarID, StructObject.NameID, new PDU.DataService.TSOServiceMessage(avatarAppearance)));
            return;

#if false
            TSOTopicUpdateMessage avatarName = new(new TSOValue_String("Bisquick"), /*MyAvatar.Avatar_Name*/ 0x4FFE270C, Avatar_DerivedID, 0x00000539);
            TSOTopicUpdateMessage avatarDescription = new(new TSOValue_String("Cheese'd to meet you."), /*MyAvatar.Avatar_Description*/ 0x54B99C9A, Avatar_DerivedID, 0x00000539);
            
            TSOTopicUpdateMessage avatarLotXY = new(new TSOValue_Uint32(0x00B800D5), /*MyAvatar.Avatar_LotXY*/ 0x7E0DD35C, Avatar_DerivedID, 0x00000539);

            var test = new TSODataServiceWrapperPDU(SendingAvatarID, DefinitionStringID, new PDU.DataService.TSOServiceMessage(avatarAppearance));
            RespondWith(new TSODataServiceWrapperPDU(SendingAvatarID, DefinitionStringID, new PDU.DataService.TSOServiceMessage(avatarName)));
            RespondWith(new TSODataServiceWrapperPDU(SendingAvatarID, DefinitionStringID, new PDU.DataService.TSOServiceMessage(avatarDescription)
            /*{
                Unknown_Flags = 0x0FE8,
                ParameterLength = 0x803B
            }*/));
            RespondWith(test);
            RespondWith(new TSODataServiceWrapperPDU(SendingAvatarID, DefinitionStringID, new PDU.DataService.TSOServiceMessage(avatarLotXY)));
#endif
        }

        private bool TSOData_TryGetObjectInstance(uint TSOData_TypeID, out TSODataServiceRuntimeObject StructObject)
        {
            if (!TryGetService<TSODataDefinitionService>(out TSODataDefinitionService? service))
                throw new InvalidOperationException("Failed to get TSODataDefinitionService.");
            dynamic obj = service.GetObject(TSOData_TypeID, out ITSODataStruct Struct);
            StructObject = new(Struct,obj);
            return obj != null;
        }

        private bool TSOData_GetDataStruct(uint TSOData_TypeID, out ITSODataStruct Struct)
        {
            if (!TryGetService<TSODataDefinitionService>(out TSODataDefinitionService? service))
                throw new InvalidOperationException("Failed to get TSODataDefinitionService.");
            return service.TryGetDataStruct(TSOData_TypeID, out Struct);
        }

        private void RespondToWithObject(TSODataServiceWrapperPDU Query, TSODataServiceRuntimeObject StructObject, uint ObjectID)
        {
            foreach (var property in TSOData_GetProperties(StructObject))
            {
                TSOTopicUpdateMessage propertyValue = new(property.Value, property.FieldInfo.NameID, ((TSODerivedStruct)StructObject.Structure).ParentID, ObjectID);
                RespondWith(new TSODataServiceWrapperPDU(Query.SendingAvatarID, StructObject.NameID, new PDU.DataService.TSOServiceMessage(propertyValue)));
            }
        }

        private IEnumerable<TSODataServiceProperty> TSOData_GetProperties(TSODataServiceRuntimeObject StructObject)
        {
            foreach (var property in StructObject.Structure.Fields)
            {
                string PropertyName = property.NameString;
                object? value = StructObject.GetFieldValue(property.NameID, out TSODataField? Field);

                //try basic value types
                TSOValueConverter.TryConvert(value, out ITSOValue? tsoValue);
                if (tsoValue == null)
                { // try struct type
                    uint typeID = Field.TypeID;

                    if (TSOData_GetDataStruct(typeID, out ITSODataStruct SubTypeStruct))
                        tsoValue = CreateStructObjectValue(new TSODataServiceRuntimeObject(SubTypeStruct, value));
                }
                if (tsoValue == null) 
                    continue; // hm, not supported yet?
                yield return new TSODataServiceProperty(Field, tsoValue);
            }
        }

        private ITSOValue CreateStructObjectValue(TSODataServiceRuntimeObject StructObject)
        {
            var properties = TSOData_GetProperties(StructObject);
            var FieldValues = properties.Select(x => new TSOValue_DataFieldValueWrapper(x.FieldInfo.NameID, x.Value));

            return new TSOValue_TSODataStructProperty(
                new TSOValue_TSODataStructPropertyWrapper(StructObject.NameID, FieldValues.ToArray()));

#if false
            return new TSOValue_TSODataStructProperty(
                new TSOValue_TSODataStructPropertyWrapper(
                    /*AvatarAppearance (Struct ID)*/ TSOData_TypeStruct.NameID,
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.BodySuitID*/0x1D530275, new TSOValue_Uint64(0x000001600000000D)),
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.SkinTone*/0x2511B29C, new TSOValue_Uint8(0x00)),
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.HeadSuitID*/0x687475BF, new TSOValue_Uint64(0x0000040F0000000D))
                    )), /*MyAvatar.Avatar_Appearance (which is STRUCT type)*/ 0xF1B92C99, Avatar_DerivedID, 0x00000539);
#endif
        }
    }
}
