
using SharedLibrary.Response;
using System.Linq.Expressions;


namespace SharedLibrary.Interface
{
    public interface IGenericInterface<T> where T : class
    {
        Task<ApiResponse> CreateAsync(T entity);
        Task<ApiResponse> UpdateAsync(T entity);
        Task<ApiResponse> DeleteAsync(T entity);    
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FindByIdAsync(int id);
        Task<T> GetByAsync(Expression<Func<T, bool>> predicate);
        
    }
}
