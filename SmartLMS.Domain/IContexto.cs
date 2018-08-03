using SmartLMS.Domain.Entities.UserAccess;
using System.Data.Entity;

namespace SmartLMS.Domain
{
    public interface IContext : Carubbi.GenericRepository.IDbContext
    {
        void Save();

        void Save(User user);

        void Update<TEntity>(TEntity oldObj, TEntity newObj) where TEntity : class;

        IDbSet<TEntity> GetList<TEntity>() where TEntity : class;
        void ConfigureAPI();

        TEntity UnProxy<TEntity>(TEntity proxyObject) where TEntity : class;
        void Reload<TEntity>(TEntity entity) where TEntity : class;
    }
}
