using AspAppLibrary.EF;
using Microsoft.EntityFrameworkCore;

namespace AspAppLibrary.Repository
{
    public class GenericRepository<T> : IRepositoryAsync<T> where T : class
    {
        private readonly ApplicationContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        
        public async Task<List<T>> GetModelListAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка при получении списка объектов. Error: {ex.Data} - {ex.Message}");
            }
        }

        public async Task<T?> GetModelByIdAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);

                if (entity != null)
                    return entity;

                return null;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка при получении объекта под id: {id}. Error: {ex.Data} - {ex.Message}");
            }
        }


        public async Task CreateModelAsync(T entity)
        {
            try
            {
                if (entity != null)
                {
                    await _dbSet.AddAsync(entity);
                    await SaveAsync();
                }
                return;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка при создании объекта. Error: {ex.Data} - {ex.Message}");
            }
        }

        public async Task UpdateModelByIdAsync(T entity)
        {
            try
            {
                if (entity != null)
                {
                    _context.Update(entity);
                    await SaveAsync();
                }
                return;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка при обновлении объекта. Error: {ex.Data} - {ex.Message}");
            }
        }

        public async Task DeleteModelByIdAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await SaveAsync();
                }
                return;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка при удалении объекта. Error: {ex.Data} - {ex.Message}");
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
