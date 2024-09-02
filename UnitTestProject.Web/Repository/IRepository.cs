using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTestProject.Web.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(int id);
        Task CreateAsync (TEntity entity);
        void Update (TEntity entity);
        void Delete (TEntity entity);

    }
}
