#define TSOTEST
#undef TSOTEST

using nio2so.DataService.Common;
using nio2so.Formats.TSOData;
using nio2so.Voltron.Core;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using nio2so.Voltron.NewImproved.Protocol.Regulator.DataService;
using nio2so.Voltron.NewImproved.Protocol.Types.DataService;
using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;
using System.Collections;
using System.Dynamic;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator
{
    /// <summary>
    /// Attribute for Methods that are handlers for TSOData structs
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class TSODataServiceTypeHandler : Attribute
    {
        public TSODataServiceTypeHandler(string StructName)
        {
            this.StructName = StructName;            
        }
        public TSODataServiceTypeHandler(string StructName, uint NameID)
        {
            this.StructName = StructName;
            this.NameID = NameID;
        }

        public string StructName { get; }
        public uint? NameID { get; } = null;
    }

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

            //REGISTER THE TYPE SERVICE
            TSODataDataServiceManager typeService = new();
            Server.Services.Register(typeService);

            //ATTEMPT TO REACH THE TSODATADEFINITION SERVICE
            if (!TryGetService(out TSODataDefinitionService? service))
                throw new InvalidOperationException("Failed to get TSODataDefinitionService.");
            
            //ENSURE SERVICE IS READY
            if (!service.Ready)
                throw new InvalidOperationException($"The {nameof(TSODataDefinitionService)} did not load correctly. The TSODataService cannot continue.");

            //ENSURE TSO VOLTRON ENUM MATCHES DATA DEFINITION -- this can cause struct ID mismatch issues if this isn't verified.
            if (!service.VerifyEnum<TSO_NewImproved_TSOData_Constants>(out string[] errors) || errors.Any())
                LogError($"The {nameof(TSO_NewImproved_TSOData_Constants)} has the following errors:\n\n{string.Join("\n",errors)}");

            //TSO TEST serialization/deserialization of ITSOValue
#if TSOTEST
            if (!Directory.GetFiles("C:/nio2so/tsotcppackets").Any())
            {
                if (service.TryGetDataStruct((uint)TSO_NewImproved_TSOData_Constants.MyAvatar, out ITSODataStruct structInfo))
                {
                    TSODataRuntimeObject data = TSODataService_GetRemoteObjectData(structInfo, 1337).Result;
                    var pdus = MakeTSODataWrapperPDUs(data, 1337, 0xFEEDF00D, 0x01);
                    int i = 0;
                    foreach (var pdu in pdus)
                    {
                        i++;
                        pdu.MakeBodyFromProperties();
                        File.WriteAllBytes(Path.Combine("C:/nio2so/tsotcppackets", $"{pdu.FriendlyPDUName} {structInfo.NameString} [{i}].dat"), pdu.Body);
                    }
                }
            }
            foreach(string file in Directory.GetFiles("C:/nio2so/tsotcppackets"))
            {
                using Stream fileStream = File.OpenRead(file);
                TSODataServiceWrapperPDU pdu = (TSODataServiceWrapperPDU)GetService<TSONewImprovedPDUFactory>().CreatePacketObjectFromDataBuffer(fileStream);
                try
                {
                    TSOTopicUpdateMessage message = (TSOTopicUpdateMessage)pdu.Message.GetMessageAs();
                    LogConsole(message.ToString());
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
#endif
        }

        const uint Avatar_TypeID = (uint)TSO_NewImproved_TSOData_Constants.Avatar; // ID of the Level Two Struct Avatar
        const uint Lot_TypeID = (uint)TSO_NewImproved_TSOData_Constants.Lot; // ID of the Level Two Struct Lot

        [TSOProtocolHandler((ushort)TSO_NewImproved_VoltronPacketTypes.DataServiceWrapperPDU)]
        public void DATA_SERVICE_REQUEST(TSOVoltronPacket PDU)
        {
            if (PDU is not TSODataServiceWrapperPDU Query)
                throw new InvalidOperationException($"Invalid PDU type for {nameof(DATA_SERVICE_REQUEST)} handler. Expected: {nameof(TSODataServiceWrapperPDU)}");            

            OnIncomingServiceMessage(Query, Query.Message);

#if false
            TSOTopicUpdateMessage avatarAppearance = new(new TSOValue_TSODataStructProperty(
                new TSOValue_TSODataStructPropertyWrapper(
                    /*AvatarAppearance (Struct ID)*/ 0x3B0430BF,
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.BodySuitID*/0x1D530275, new TSOValue_Uint64(0x000001600000000D)),
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.SkinTone*/0x2511B29C, new TSOValue_Uint8(0x00)),
                    new TSOValue_DataFieldValueWrapper(/*AvatarAppearance.HeadSuitID*/0x687475BF, new TSOValue_Uint64(0x0000040F0000000D))
                    )), /*MyAvatar.Avatar_Appearance (which is STRUCT type)*/ 0xF1B92C99, Avatar_TypeID, 0x00000539);
            RespondWith(new TSODataServiceWrapperPDU(Query.SendingAvatarID, StructObject.NameID, new PDU.DataService.TSOServiceMessage(avatarAppearance)));
            return;

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
        /// <summary>
        /// Redirects to the appropriate handler function for the given <see cref="ITSOMessage"/> <paramref name="ServiceMessage"/> type
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="ServiceMessage"></param>
        private void OnIncomingServiceMessage(TSODataServiceWrapperPDU Query, TSOServiceMessage ServiceMessage)
        {
            ITSOMessage MessageContent = ServiceMessage.GetMessageAs();

            switch (MessageContent.GetMessageType())
            {
                case TSO_NewImproved_TSOMessageType_CLSIDs.cTSONetMessageStandard:
                    bool HasEmbeddedData = ((TSONetMessageStandard)MessageContent).HasEmbeddedMessage;
                    if (HasEmbeddedData)
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        OnIncomingServiceMessage(Query, ((TSONetMessageStandard)MessageContent).EmbeddedServiceMessage);
#pragma warning restore CS8604 // Possible null reference argument.
                        return;
                    }
                    else
                        OnIncomingNetMessageStandard(Query, (TSONetMessageStandard)MessageContent);
                    break;
                case TSO_NewImproved_TSOMessageType_CLSIDs.cTSOTopicUpdateMessage:                    
                    OnIncomingTopicUpdateMessage(Query, (TSOTopicUpdateMessage)MessageContent);
                    break;
            }
        }
        /// <summary>
        /// Called when an incoming <see cref="TSOServiceMessage"/> is a <see cref="TSOTopicUpdateMessage"/>
        /// <para/> <inheritdoc cref="TSOTopicUpdateMessage"/>
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="ServiceMessage"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        private void OnIncomingTopicUpdateMessage(TSODataServiceWrapperPDU Query, TSOTopicUpdateMessage ServiceMessage)
        {
            // Layout the exact request parameters
            TSO_NewImproved_TSOData_Constants StructID = (TSO_NewImproved_TSOData_Constants)ServiceMessage.GetTSODataDefinitionDetail_TypeID();
            uint ObjectID = ServiceMessage.GetTSODataDefinitionDetail_ObjectID();
            TSO_NewImproved_TSOData_Constants FieldID = (TSO_NewImproved_TSOData_Constants)ServiceMessage.GetTSODataDefinitionDetail_FieldID();

            //Get requested data structure info
            if (!GetService<TSODataDefinitionService>().TryGetDataStruct((uint)StructID, out ITSODataStruct structInfo))
                throw new InvalidOperationException($"Cannot locate the {nameof(ITSODataStruct)}: {StructID:X} (Query: {Query.DefinitionStringID:X})");

            // find the field of the TSO Data Struct we're updating
            TSODataField? fieldInfo = structInfo.GetFieldByID((uint)FieldID);
            if (fieldInfo == null)
                throw new NullReferenceException(nameof(fieldInfo));

            // values from the TSOClient sometimes are not formatted as expected, we need to sanitize the values using Controllers
            // these controllers take the query and the RuntimeDataObject and ensure things are well-formatted for our Data Service.

            // Value verification code begins here
            ITSOValue TSOValue = ServiceMessage.Value;
            if (GetService<TSODataDefinitionService>().TryGetDataStruct((uint)fieldInfo.TypeID, out ITSODataStruct valueStructInfo))
            { // data structs require verification
                object? valueArray = TSOValue_SafeGetValue(TSOValue); // try to read the value in the ITSOValue
                if (valueArray == null)
                    throw new NullReferenceException("TSOValue was null."); // null value
                if (valueArray is not IEnumerable && valueArray is not string) // if its a vector of objects, handle this case
                    valueArray = new object[] { valueArray }; // ensure it is a vector ... this gets checked after verification
                List<ITSOValue> verifiedValues = new();
                foreach (object arbiraryValue in (IEnumerable)valueArray)
                { // for each value in this list, verify it
                    TSODataRuntimeObject StructObject = null;
                    if (arbiraryValue is TSODataRuntimeObject) // is it already a runtime struct? remember that struct object types are expected to be this, but sometimes the client doesn't do this.
                        StructObject = (TSODataRuntimeObject)arbiraryValue;
                    else // incoming value struct is supposed to be one but isn't, make one from what data we do have
                        GetService<TSODataDefinitionService>().CreateObject(valueStructInfo, out StructObject, arbiraryValue);           
                    //sanitize the value using Controllers
                    if (TSOData_InvokeControllers(Query, StructObject, TSODataServiceControllerBase.TSODataServiceHandlerDataDirection.FromTSOClient))
                        verifiedValues.Add(CreateStructObjectValue(StructObject)); // add the sanitized value to the list
                }
                if (verifiedValues.Count == 1) // is it a Single Item list? if so just return the value
                    TSOValue = verifiedValues[0];
                else if (verifiedValues.Count > 1) // multiple objects must be wrapped into a Vector
                    TSOValue = ConvertFieldPropertyToTSOValue(fieldInfo, verifiedValues);
                else throw new InvalidDataException("Verification Failed -- no values were ultimately returned from the Controller.");
            }

            // UPLOAD DATA TO THE REMOTE SERVICE
            bool successful = TSODataService_SetRemoteObjectDataField(structInfo, fieldInfo, ObjectID, TSOValue).Result; // do remote data service query now

            // SUCCESS. NOW REQUEST THE UPDATED VALUE FROM THE DATA SERVICE
            if (successful)
                OnIncomingStandardQuery(
                    new TSODataServiceWrapperPDU(Query.SendingAvatarID, (uint)StructID, Query.Message), [0,ObjectID]);
        }

        private void OnIncomingNetMessageStandard(TSODataServiceWrapperPDU Query, TSONetMessageStandard ServiceMessage) => 
            OnIncomingStandardQuery(Query, ServiceMessage.GetDataValues());

        private void OnIncomingStandardQuery(TSODataServiceWrapperPDU Query, params uint[] Values)
        {
            if (!Values.Any())
                throw new ArgumentException($"The provided {nameof(Values)} attached to this {nameof(OnIncomingStandardQuery)} are: none.");

            //objectID is historically Data2 (last) ... but could change later!
            uint ObjectID = Values.Last();

            //REMOTE DATA SERVICE REQUEST

            TSODataRuntimeObject StructObject = null;

            //Get requested data structure info
            if (GetService<TSODataDefinitionService>().TryGetDataStruct((uint)Query.DefinitionStringID, out ITSODataStruct structInfo))
                StructObject = TSODataService_GetRemoteObjectData(structInfo, ObjectID).Result; // do remote data service query now

            if (StructObject == null)
            { // Data Service returned null or the structure was not found.
                throw new InvalidDataException($"nio2so Data Service returned null or the structure was not found.");
                TSOData_TryGetObjectInstance((uint)Query.DefinitionStringID, out StructObject);
            }
            // invoke the controllers with the data service direction to double-check the data
            if (TSOData_InvokeControllers(Query, StructObject, TSODataServiceControllerBase.TSODataServiceHandlerDataDirection.FromDataService))
                RespondToWithObject(Query, StructObject, ObjectID);            
        }

        /// <summary>
        /// Invokes a <see cref="TSODataServiceControllerBase"/> to sanitize the value provided from the Data Service or TSO depending on <paramref name="Direction"/>
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="StructObject"></param>
        /// <param name="Direction"></param>
        /// <returns></returns>
        private bool TSOData_InvokeControllers(TSODataServiceWrapperPDU Query, TSODataRuntimeObject StructObject, TSODataServiceControllerBase.TSODataServiceHandlerDataDirection Direction)
        {
            if (StructObject != null)
            {
                TSODataServiceControllerBase.HandlerContext ctx = new(Query, StructObject, Direction);

                var typeService = GetService<TSODataDataServiceManager>();
                try
                {
                    typeService.Invoke(this, ctx);
                    return true;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Makes a data service request to the nio2so Data Service for the specified data object
        /// </summary>
        /// <returns></returns>
        private async Task<TSODataRuntimeObject> TSODataService_GetRemoteObjectData(ITSODataStruct Struct, uint ObjectID)
        {
            async Task<object?> DoRemote()
            {
                if (!TryGetService<nio2soVoltronDataServiceClient>(out nio2soVoltronDataServiceClient? service))
                    throw new InvalidOperationException($"Failed to get {nameof(nio2soDataServiceClient)}.");
                HTTPServiceClientBase.HTTPServiceResult<object> result = await service.GetTSODataServiceObjectByID("1.1097.1.0", Struct.NameString, ObjectID, Struct.NameID);
                if (!result.IsSuccessful)
                    throw new InvalidOperationException("Data Service returned: " + result.FailureReason);
                return result.Result;
            }
            object? instance = await DoRemote();
            if (instance is null)
                throw new NullReferenceException($"Returned instance data from data service is null.");
            return new TSODataRuntimeObject(Struct, instance);
        }
        /// <summary>
        /// Makes a data service request to the nio2so Data Service for the specified data object
        /// </summary>
        /// <returns></returns>
        private async Task<bool> TSODataService_SetRemoteObjectDataField(ITSODataStruct Struct, TSODataField FieldInfo, uint ObjectID, ITSOValue TSOValue)
        {            
            object? value = TSOValue_SafeGetValue(TSOValue);            

            if (value == null)
            {
                LogError($"Submitted value to change in the data service was NULL. {Struct} {ObjectID}: null");
                return false;
            }

            if (value is TSODataRuntimeObject runtimeObject)
                value = runtimeObject.Instance;

            async Task<HttpResponseMessage> DoRemote()
            {
                if (!TryGetService<nio2soVoltronDataServiceClient>(out nio2soVoltronDataServiceClient? service))
                    throw new InvalidOperationException($"Failed to get {nameof(nio2soDataServiceClient)}.");
                return await service.PostTSODataServiceObjectFieldByID("1.1097.1.0", Struct.NameString, FieldInfo.NameString, ObjectID, value, Struct.NameID, FieldInfo.NameID);
            }
            return (await DoRemote()).IsSuccessStatusCode;
        }

        private bool TSOData_TryGetObjectInstance(uint TSOData_TypeID, out TSODataRuntimeObject StructObject)
        {
            if (!TryGetService(out TSODataDefinitionService? service))
                throw new InvalidOperationException("Failed to get TSODataDefinitionService.");
            dynamic obj = service.GetStructObject(TSOData_TypeID, out ITSODataStruct Struct);
            StructObject = new(Struct,obj);
            return obj != null;
        }

        private bool TSOData_GetDataStruct(uint TSOData_TypeID, out ITSODataStruct Struct) => TryGetService(out TSODataDefinitionService? service)
                ? service.TryGetDataStruct(TSOData_TypeID, out Struct)
                : throw new InvalidOperationException("Failed to get TSODataDefinitionService.");

        private TSODataServiceWrapperPDU MakeTSODataWrapperPDUForProperty(ITSODataStruct Struct, TSODataServiceProperty Property, uint ObjectID, uint SendingAvatarID, uint SequenceID)
        {
            uint StructID = Struct.GetBaseStructID();
            TSOTopicUpdateMessage propertyValue = new(Property.Value, Property.FieldInfo.NameID, StructID, ObjectID);
            return new TSODataServiceWrapperPDU(SendingAvatarID, StructID, new PDU.DataService.TSOServiceMessage(propertyValue, SequenceID));
        }
        private TSODataServiceWrapperPDU MakeTSODataWrapperPDUForProperty(ITSODataStruct Struct, uint FieldID, uint ObjectID, ITSOValue Value, uint SendingAvatarID, uint SequenceID)
        {
            TSODataServiceProperty property = TSOData_GetProperty(Struct, FieldID, Value);
            return MakeTSODataWrapperPDUForProperty(Struct, property, ObjectID, SendingAvatarID, SequenceID);
        }
        private TSODataServiceWrapperPDU MakeTSODataWrapperPDUForProperty(TSODataRuntimeObject StructObject, uint FieldID, uint ObjectID, uint SendingAvatarID, uint SequenceID)
        {
            TSODataServiceProperty property = TSOData_GetProperty(StructObject, FieldID);
            return MakeTSODataWrapperPDUForProperty(StructObject.Structure, property, ObjectID, SendingAvatarID, SequenceID);
        }

        private IEnumerable<TSODataServiceWrapperPDU> MakeTSODataWrapperPDUs(TSODataRuntimeObject StructObject, uint ObjectID, uint SendingAvatarID, uint SequenceID)
        {
            foreach (var property in TSOData_GetProperties(StructObject))
                yield return MakeTSODataWrapperPDUForProperty(StructObject, property.FieldInfo.NameID, ObjectID, SendingAvatarID, SequenceID);
        }

        private void RespondTo(TSODataServiceWrapperPDU Query, TSODataServiceWrapperPDU Response)
        {
            Response.SendingAvatarID = Query.SendingAvatarID;
            Response.Message.SequenceID = Query.Message.SequenceID;
            RespondWith(Response);
        }
        private void RespondToWithObject(TSODataServiceWrapperPDU Query, TSODataRuntimeObject StructObject, uint ObjectID) => 
            RespondWithAll(MakeTSODataWrapperPDUs(StructObject, ObjectID, Query.SendingAvatarID, Query.Message.SequenceID));

        internal ITSOValue ConvertFieldPropertyToTSOValue(TSODataField property, object? Value)
        {
            if (property == null)
                throw new Exception($"{property.TypeID:X} ({property}) could not be loaded from TSODataDefinition file.");

            void ThrowNotImplemented()
            {
                string error = $"{property.TypeID} {property.TypeString} not implemented yet! ";
                throw new NotImplementedException(error);
            }

            if (Value is null)
                throw new ArgumentException(nameof(Value) + " is null");

            if (Value is TSODataRuntimeObject runtimeObject)
                Value = runtimeObject.Instance;

            ITSOValue tsoValue = default;

            uint typeID = property.TypeID;
            switch (property.Classification)
            {
                case TSODataFieldClassification.SingleField:
                    {
                        //attempt to locate this struct type, if it exists
                        bool structFound = TSOData_GetDataStruct(typeID, out ITSODataStruct SubTypeStruct);
                        if (!structFound)
                        { // struct not found, try basic value types
                            if (!TSOValueConverter.TryConvert(property.TypeString, Value, out tsoValue))
                                ThrowNotImplemented();
                        }
                        else
                        { // struct type found, try struct type value creation
                            tsoValue = CreateStructObjectValue(new TSODataRuntimeObject(SubTypeStruct, Value));
                        }                        
                    }
                    break;
                case TSODataFieldClassification.TypedList:
                    {
                        //attempt to locate this struct type, if it exists
                        var structFound = TSOData_GetDataStruct(typeID, out ITSODataStruct SubTypeStruct);
                        if (!structFound)
                        { // try basic types: bool,uint,etc.
                            TSOValueConverter.TryConvertArray(property.TypeString, Value, out ITSOValueVector? tsoValueVector);
                            tsoValue = tsoValueVector;
                        }
                        else
                        { // try TSODataStructs vector
                            IEnumerable tsoObjectList = null;
                            if (Value is IEnumerable && Value is not string) // list of objects returned, good to go.
                                tsoObjectList = (IEnumerable)Value;
                            else if (Value is ExpandoObject) // single object returned ... make into a list
                                tsoObjectList = new ExpandoObject[] { (ExpandoObject)Value };

                            if (Value == null)
                                throw new InvalidDataException($"The value provided is not a list type.");

                            // ensure that all incoming objects to this function are runtime objects
                            tsoValue = new TSOValue_PropertyVector(tsoObjectList.
                                Cast<object>().Select(x => 
                                CreateStructObjectValue(x is TSODataRuntimeObject ? (TSODataRuntimeObject)x : new TSODataRuntimeObject(SubTypeStruct, x))).ToArray());
                        }
                    }
                    break;
                case TSODataFieldClassification.Map:
                    ThrowNotImplemented();

                    TSOValueConverter.TryConvertMap(property.TypeString, Value, out ITSOValueMap? tsoValueMap);
                    tsoValue = tsoValueMap;
                    break;
            }
            
            if (tsoValue == null)
            {
                ThrowNotImplemented();
            }
            return tsoValue;
        }

        private TSODataServiceProperty TSOData_GetProperty(TSODataRuntimeObject StructObject, uint FieldID)
        {
            TSODataField? fieldInfo = StructObject.Structure.GetFieldByID(FieldID);
            return TSOData_GetProperty(StructObject.Structure, fieldInfo, ConvertFieldPropertyToTSOValue(fieldInfo, StructObject.GetFieldValue(FieldID, out _)));
        }
        private TSODataServiceProperty TSOData_GetProperty(ITSODataStruct Structure, uint FieldID, ITSOValue Value) =>
            TSOData_GetProperty(Structure, Structure.GetFieldByID(FieldID), Value);
        private TSODataServiceProperty TSOData_GetProperty(ITSODataStruct Struct, TSODataField property, ITSOValue Value)
        {
            bool verifyValue()
            {
                switch (property.Classification)
                {
                    case TSODataFieldClassification.SingleField:
                        {
                            if (Value is TSOValue_TSODataStructProperty dataObject)
                                return property.TypeID == (uint)dataObject.Value.TSODataDefinition_TypeStructID;
                            //not a dataobject
                            return property.TypeID == (uint)Value.TypeClassID;
                        }
                    case TSODataFieldClassification.TypedList:
                        {
                            if (Value is not ITSOValueVector vec) // not a list
                                return false;
                            else if (vec.GetItemCount() == 0) // empty lists cant be type tested
                                return true;
                            if (Value is TSOValue_PropertyVector dataObjectVec)
                                return property.TypeID == (uint)dataObjectVec.GetItemCollection().Cast<TSOValue_TSODataStructProperty>().First().Value.TSODataDefinition_TypeStructID;
                            //not a dataobject
                            return property.TypeID == (uint)Value.TypeClassID;
                        }
                    default:
                        return false;
                }
            }

            ITSOValue? value = Value;
            if (value == null)
                throw new NullReferenceException(nameof(value));
        
            if (property == null)
                throw new NullReferenceException(nameof(property));

            string PropertyName = property.NameString;

            if (property == null)
                throw new Exception($"{property.TypeID:X} ({property}) could not be loaded from TSODataDefinition file.");

            //verify value is assignable

            //if (!verifyValue())
            { // messy, rewrite later.
                if (property.Classification == TSODataFieldClassification.TypedList)
                {
                    if (value is not ITSOValueVector)
                    { // not a vector, make it one
                        value = ConvertFieldPropertyToTSOValue(property, TSOValue_SafeGetValue(value));
                    }
                }
            }

            return new TSODataServiceProperty(property, value);
        }

        private IEnumerable<TSODataServiceProperty> TSOData_GetProperties(TSODataRuntimeObject StructObject)
        {
            List<TSODataServiceProperty> properties = new();
            foreach (var property in StructObject.Structure.Fields)
            {
                try
                {
                    properties.Add(TSOData_GetProperty(StructObject, property.NameID));
                }
                catch(Exception ex)
                {
                    LogError(ex.Message + " Skipping...");
                }
            }
            return properties;
        }

        private TSOValue_TSODataStructProperty CreateStructObjectValue(TSODataRuntimeObject StructObject)
        {
            var properties = TSOData_GetProperties(StructObject);
            var FieldValues = properties.Select(x => new TSOValue_DataFieldValueWrapper(x.FieldInfo.NameID, x.Value));

            return new TSOValue_TSODataStructProperty(new TSOValue_TSODataStructPropertyWrapper(StructObject.NameID, FieldValues.ToArray()));

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

        // automatically uses the appropriate overload of the GetValue() function
        object? TSOValue_SafeGetValue(ITSOValue TSOValue)
        {
            if (TSOValue is TSOValue_TSODataStructProperty dataStructProperty)
                return dataStructProperty.GetValue(GetService<TSODataDefinitionService>()); // get TSODataRuntimeObject with the correct field names
            else if (TSOValue is TSOValue_PropertyVector dataStructPropVec)
                return dataStructPropVec.Value.Select(x => TSOValue_SafeGetValue(x)).ToArray();
            else return TSOValue.GetValue();
        }

    }
}
