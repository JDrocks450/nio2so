using System.Text.Json.Serialization;

namespace nio2so.CrashHandler
{
    /// <summary>
    /// A serializable type that can load error details for when the program that crashed saves error details to file.
    /// </summary>
    [Serializable] public class ErrorDetail
    {
        public const string FNAME = "errordetail.json";

        public ErrorHandlerArgs Arguments { get; set;  }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public string ProgramName { get; set; }
        public DateTime ErrorTime { get; set; }
    }
}
