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
    [Route("tso/")]
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

        [HttpGet("{Version}/data/{DataDefinitionStructName}")]
        public ActionResult TSODataServiceRequest(string Version, string DataDefinitionStructName, [FromQuery] TSODataServiceQuery Query) 
        
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
