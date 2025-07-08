namespace nio2so.Database
{
    /// <summary>
    /// The <see cref="DatabaseService"/> can be passed to the nio2so server cluster to serve data for the given server configuration
    /// </summary>
    public class DatabaseService
    {
        /// <summary>
        /// The directory on the hard drive where the databases
        /// </summary>
        public string HomeDirectory { get; set; }

        public DatabaseService(string HomeDirectory) {
            
        }
    }
}
