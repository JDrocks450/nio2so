using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API
{
    public abstract class DataServiceControllerBase : ControllerBase
    {
        protected ActionResult<TResult> GetObjectByID<TQuery,TResult>(Func<TQuery, TResult> QueryFunction, TQuery ID)
        {
            TResult? objectData = default;
            try
            {
                objectData = QueryFunction(ID);
            }
            catch (KeyNotFoundException ke)
            {
                return NotFound(ke.Message);
            }
            catch (FileNotFoundException fe)
            {
                return NotFound(fe.Message);
            }
            if (objectData == null)
                return NotFound($"{ID} was not found.");
            return objectData;
        }
        protected async Task<ActionResult<TResult>> GetObjectByIDAsync<TQuery, TResult>(Func<TQuery, Task<TResult>> QueryFunction, TQuery ID)
        {
            TResult? objectData = default;
            try
            {
                objectData = await QueryFunction(ID);
            }
            catch (KeyNotFoundException ke)
            {
                return NotFound(ke.Message);
            }
            catch (FileNotFoundException fe)
            {
                return NotFound(fe.Message);
            }
            if (objectData == null)
                return NotFound($"{ID} was not found.");
            return objectData;
        }
        /// <summary>
        /// Create a new <see cref="HttpResponse"/> using the <see cref="ControllerBase.Response"/> property
        /// writing the byte array into the body of the response and setting <paramref name="StatusCode"/>
        /// </summary>
        /// <param name="ContentBytes"></param>
        /// <param name="MIMEString"></param>
        /// <param name="StatusCode"></param>
        /// <returns></returns>
        protected async Task MIMEResponse(byte[]? ContentBytes, string MIMEString = "application/octet-stream", int StatusCode = 200)
        {
            if (ContentBytes == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                await Response.WriteAsync("The requested resource file could not be found.");
                return;
            }
            Response.Headers.Add(HeaderNames.ContentType, MIMEString);
            Response.StatusCode = StatusCode;
            await Response.Body.WriteAsync(ContentBytes);
        }
    }
}
