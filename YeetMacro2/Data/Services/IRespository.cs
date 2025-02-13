using System.Linq.Expressions;

namespace YeetMacro2.Data.Services;
//https://codewithshadman.com/repository-pattern-csharp/
//https://www.youtube.com/watch?v=rtXpYpZdOzM&feature=youtu.be
//Benefits - Minimized duplicate query logic and decouples your application from persistance frameworks
//Queries only exist in the Repository
public interface IRepository<TEntity> where TEntity : class
{
    void Delete(TEntity entityToDelete);
    void Delete(object id);
    IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Expression<Func<TEntity, object>> includePropertyExpression = null,
        bool noTracking = false);
    TEntity GetById(object id, Expression<Func<TEntity, object>> includePropertyExpression = null);
    void Insert(TEntity entity);
    void Update(TEntity entityToUpdate);
    void Save();
}