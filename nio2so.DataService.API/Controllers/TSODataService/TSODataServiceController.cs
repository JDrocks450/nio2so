using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Types.TSOData;
using nio2so.Formats;
using nio2so.Formats.TSOData;

namespace nio2so.DataService.API.Controllers.TSODataService
{
    /// <summary>
    /// This component is intended to be used with The Sims Online versions that make use of the TSO Data Service,
    /// and it is build around the following versions:
    /// <para/><inheritdoc cref="TSOVersions.TSO_NI_1_1097_1_0"/>
    /// </summary>
    [Route("api/tso/")]
    [ApiController]
    public class TSODataServiceController : DataServiceControllerBase
    {
        TSODataFile tsoDataDefinition;
        TSODataObjectsDataService dataService => APIDataServices.TSOData_DataService;

        public TSODataServiceController(ILogger<TSODataServiceController> Logger)
        {
            string TSODataPath = APIDataServices.ConfigService.GetCurrentSettings().VoltronSettings.TSODataFilePath;
            //Load the data definition
            tsoDataDefinition = new TSODataImporter().ImportFromFile(TSODataPath);
        }

        [HttpGet("{Version}/data/object/{DataDefinitionStructName}")]
        public ActionResult TSODataServiceObjectGetRequest(string Version, string DataDefinitionStructName, [FromQuery] TSODataServiceQuery Query) 
        
        {
            ITSODataStruct RequestedDataType = null;

            if (!string.IsNullOrWhiteSpace(Query.StructID) && Query.GetDataDefinitionStructID()!=0)
                tsoDataDefinition.TryGetStructByID(Query.GetDataDefinitionStructID(), out RequestedDataType);
            else if (DataDefinitionStructName != null)             
                tsoDataDefinition.TryGetStructByName(DataDefinitionStructName, out RequestedDataType);

            //detect errors
            if (RequestedDataType == null)
                return NotFound();
            if (RequestedDataType.NameString != DataDefinitionStructName)
                return Problem("Internal server error occured. This could be due to your StructID being incorrect.");


            TSODataRuntimeObject obj = GetRequestedData(RequestedDataType, Query.GetObjectID());
            return Ok(obj.Instance);
        }

        [HttpGet("{Version}/data/object/{DataDefinitionStructName}/field/{DataFieldName}")]
        public ActionResult TSODataServiceFieldGetRequest(string Version, string DataDefinitionStructName, string DataFieldName, [FromQuery] TSODataServiceQuery Query)

        {
            ITSODataStruct RequestedDataType = null;
            TSODataField field = null;

            //**check struct info

            if (!string.IsNullOrWhiteSpace(Query.StructID) && Query.GetDataDefinitionStructID() != 0)
                tsoDataDefinition.TryGetStructByID(Query.GetDataDefinitionStructID(), out RequestedDataType);
            else if (DataDefinitionStructName != null)
                tsoDataDefinition.TryGetStructByName(DataDefinitionStructName, out RequestedDataType);

            //detect errors
            if (RequestedDataType == null)
                return NotFound();
            if (RequestedDataType.NameString != DataDefinitionStructName)
                return Problem("Internal server error occured. This could be due to your StructID being incorrect.");

            //**check field info

            if (!string.IsNullOrWhiteSpace(Query.FieldID))
                field = RequestedDataType.GetFieldByID(Query.GetFieldID());
            if (!string.IsNullOrWhiteSpace(DataFieldName) && field == null)
                field = RequestedDataType.GetFieldByName(DataFieldName);
            if (field == null)
                return NotFound();

            //get object
            TSODataRuntimeObject obj = GetRequestedData(RequestedDataType, Query.GetObjectID());

            //return field
            object? instanceFieldValue = obj.GetFieldValue(field);

            return Ok(instanceFieldValue);
        }

        [HttpPost("{Version}/data/object/{DataDefinitionStructName}/field/{DataFieldName}")]
        public async Task<ActionResult> TSODataServicePostFieldRequest(string Version, string DataDefinitionStructName, string DataFieldName, [FromQuery] TSODataServiceQuery Query, [FromBody] object Value)

        {
            ITSODataStruct RequestedDataType = null;
            TSODataField field = null;

            //**check struct info

            if (!string.IsNullOrWhiteSpace(Query.StructID) && Query.GetDataDefinitionStructID() != 0)
                tsoDataDefinition.TryGetStructByID(Query.GetDataDefinitionStructID(), out RequestedDataType);
            else if (DataDefinitionStructName != null)
                tsoDataDefinition.TryGetStructByName(DataDefinitionStructName, out RequestedDataType);

            //detect errors
            if (RequestedDataType == null)
                return NotFound();
            if (RequestedDataType.NameString != DataDefinitionStructName)
                return Problem("Internal server error occured. This could be due to your StructID being incorrect.");

            //**check field info

            if (!string.IsNullOrWhiteSpace(Query.FieldID))
                field = RequestedDataType.GetFieldByID(Query.GetFieldID());
            if (!string.IsNullOrWhiteSpace(DataFieldName) && field == null)
                field = RequestedDataType.GetFieldByName(DataFieldName);
            if (field == null)
                return NotFound();

            bool success = await TrySetDataObjectField(RequestedDataType, field, Query.GetObjectID(), Value);

            return success ? Ok() : Problem();
        }

        private Task<bool> TrySetDataObjectField(ITSODataStruct Struct, TSODataField Field, uint ObjectID, object value) => dataService.TrySetTSODataObjectProperty(Struct, Field, ObjectID, value);

        private TSODataRuntimeObject GetRequestedData(ITSODataStruct DataStructure, uint ObjectID)
        {
            TSODataRuntimeObject databaseData = Database_GetObject(DataStructure, ObjectID);
            return TSODataRuntimeObject.CopyTo(DataStructure, databaseData); // write clone function
        }

        private TSODataRuntimeObject Database_GetObject(ITSODataStruct RequestedType, uint ObjectID)
        {
            ITSODataStruct DatabaseStructType = RequestedType;
            if (DatabaseStructType.IsDerived) // Get the parent data type -- this is what we would store in the database.
            {
                uint databaseTypeID = (DatabaseStructType as TSODerivedStruct).ParentID;
                //**get requested struct type
                if (!tsoDataDefinition.TryGetStructByID(databaseTypeID, out DatabaseStructType) || DatabaseStructType == null)
                    throw new InvalidDataException($"Cannot get TSOData struct {databaseTypeID:X} from the TSODataDefinition file.");
            }

            TSODataRuntimeObject Object = null;

            //attempt to load this data from the database libraries
            if (!dataService.TryGetTSODataObject(DatabaseStructType.NameString, ObjectID, out var StoredValue))
            { // object value does not exist yet -- create a default value now and save that to the database.
                if (!TSOData_TryGetObjectInstance(DatabaseStructType.NameID, out Object))
                    throw new InvalidDataException($"Cannot load TSOData Object {DatabaseStructType} with ID: {ObjectID}. Object creation failed.");
                dataService.TrySetTSODataObject(ObjectID, Object);
            }
            else
            {
                if (DatabaseStructType.NameID != StoredValue.TSOData_DataStructID)
                    throw new InvalidDataException("Catastropic failure: Database data structure somehow doesn't match returned data structure. Database corruption likely.");
                Object = new(RequestedType,StoredValue.TSODataObject);
            }

            if (Object == null)
                throw new NullReferenceException(nameof(Object));

            return Object;
        }

        private bool TSOData_TryGetObjectInstance(uint TSOData_TypeID, out TSODataRuntimeObject StructObject) => 
            tsoDataDefinition.TryGetObject(TSOData_TypeID, out StructObject);
    }
}
