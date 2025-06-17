namespace AspAppLibrary.Repository
{
    public interface IRepositoryAsync<T> where T : class
    {
        Task<List<T>> GetModelListAsync();
        Task<T?> GetModelByIdAsync(int id);
        Task CreateModelAsync(T item);
        Task UpdateModelByIdAsync(T entity);
        Task DeleteModelByIdAsync(int id);
        Task SaveAsync();
    }
}
