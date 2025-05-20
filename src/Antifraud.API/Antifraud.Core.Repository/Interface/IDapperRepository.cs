namespace Antifraud.Core.Repository.Interface
{
    public interface IDapperRepository<T> where T : class
    {
        Task<int> ExecuteAsync(string query, object parameters = null);
        Task<IEnumerable<T>> QueryAsync(string query, object parameters = null);
        Task<T> QueryFirstOrDefaultAsync(string query, object parameters = null);
    }
}
