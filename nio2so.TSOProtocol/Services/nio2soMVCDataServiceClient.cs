using Microsoft.Extensions.Configuration;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using System.Net.Http.Json;

namespace nio2so.TSOHTTPS.Protocol.Services
{
    /// <summary>
    /// The <see cref="nio2soDataServiceClient"/> but as an MVC service
    /// <inheritdoc cref="nio2soDataServiceClient"/>
    /// </summary>
    public class nio2soMVCDataServiceClient : nio2soDataServiceClient
    {
        /// <summary>
        /// Creates a new <see cref="nio2soMVCDataServiceClient"/>
        /// <para><inheritdoc cref="APIAddress"/></para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="appSettings">Requires your appSettings.json to contain the APIAddress property to find the nio2so Api server</param>
        public nio2soMVCDataServiceClient(HttpClient client, IConfiguration appSettings) : 
            base(client, new(appSettings.GetValue<string>("APIAddress"))) { }
    }
}
