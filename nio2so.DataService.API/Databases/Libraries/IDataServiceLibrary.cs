namespace nio2so.DataService.API.Databases.Libraries
{
    internal interface IDataServiceLibrary
    {
        Task Load();
        Task Save();
        Task InvokeEnsureDefaultValues();
    }
}
