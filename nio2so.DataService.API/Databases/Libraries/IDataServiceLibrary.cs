namespace nio2so.DataService.API.Databases.Libraries
{
    public interface IDataServiceLibrary
    {
        Task Load();
        Task Save();
        Task InvokeEnsureDefaultValues();
    }
}
