using Microsoft.AspNetCore.Mvc;
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
    [Route("tso/data")]
    [ApiController]
    public class TSODataServiceController : DataServiceControllerBase
    {
        TSODataFile tsoDataDefinition;

        public TSODataServiceController(ILogger<TSODataServiceController> Logger)
        {
            string TSODataPath = APIDataServices.ConfigService.GetCurrentSettings().VoltronSettings.TSODataFilePath;
            //Load the data definition
            tsoDataDefinition = new TSODataImporter().ImportFromFile(TSODataPath);
        }

        [HttpGet("{Version}/{DataDefinitionStructName}")]
        public ActionResult TSODataServiceRequest(string Version, string DataDefinitionStructName, [FromQuery] TSODataServiceQuery Query) 
        {
            ITSODataStruct RequestedDataType = null;

            if (!string.IsNullOrWhiteSpace(Query.StructID))
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
            //**this is where you would normally load the data
            uint databaseTypeID = DataStructure.NameID;
            if (DataStructure.IsDerived) // Get the parent data type -- this is what we would store in the database.            
                databaseTypeID = (DataStructure as TSODerivedStruct).ParentID;
            if (!Database_TryGetObject(databaseTypeID, ObjectID, out TSODataRuntimeObject? databaseData) || databaseData == null)
                throw new InvalidDataException("Not found.");
            return TSODataRuntimeObject.Clone(DataStructure, databaseData); // write clone function
        }

        private bool Database_TryGetObject(uint DataStructID, uint ObjectID, out TSODataRuntimeObject Object)
        {
            //TEMPORARY
            TSOData_TryGetObjectInstance(DataStructID, out Object);
            return true;
        }

        private bool TSOData_TryGetObjectInstance(uint TSOData_TypeID, out TSODataRuntimeObject StructObject) => 
            tsoDataDefinition.TryGetObject(TSOData_TypeID, out StructObject);
    }
}
