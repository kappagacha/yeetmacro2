using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace YeetMacro2.Data.Services;

public class YeetMacroEfRepository<TEntity>(YeetMacroDbContext context) : EfRepository<YeetMacroDbContext, TEntity>(context)
    where TEntity : class
{
}

//https://codewithshadman.com/repository-pattern-csharp/
public class EfRepository<TContext, TEntity>(TContext context) : IRepository<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    protected TContext context = context;
    internal DbSet<TEntity> dbSet = context.Set<TEntity>();

    public virtual IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Expression<Func<TEntity, object>> includePropertyExpression = null,
        bool noTracking = false)
    {

        IQueryable<TEntity> query = dbSet;

        // https://learn.microsoft.com/en-us/ef/core/querying/tracking
        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includePropertyExpression != null)
        {
            List<PropertyInfo> includeProperties = ReflectionHelper.GetMultiPropertyInfo<TEntity, object>(includePropertyExpression);
            foreach (PropertyInfo propertyInfo in includeProperties)
            {
                query = query.Include(propertyInfo.Name);
            }
        }

        if (orderBy != null)
        {
            return [.. orderBy(query)];
        }
        else
        {
            return [.. query];
        }
    }

    public virtual TEntity GetById(object id, Expression<Func<TEntity, object>> includePropertyExpression = null)
    {
        var entity = dbSet.Find(id);

        if (entity != null && includePropertyExpression != null)
        {
            List<PropertyInfo> includeProperties = ReflectionHelper.GetMultiPropertyInfo<TEntity, object>(includePropertyExpression);
            if (includePropertyExpression != null && includeProperties.Count > 0)
            {
                foreach (PropertyInfo propertyInfo in includeProperties)
                {
                    switch (context.Entry(entity).Member(propertyInfo.Name))
                    {
                        case ReferenceEntry:
                            context.Entry(entity).Reference(propertyInfo.Name).Load();
                            break;
                        case CollectionEntry:
                            context.Entry(entity).Collection(propertyInfo.Name).Load();
                            break;
                    }
                }
            }
        }

        return entity;
    }

    public virtual void Insert(TEntity entity)
    {
        dbSet.Add(entity);
    }

    public virtual void Delete(object id)
    {
        TEntity entityToDelete = dbSet.Find(id);
        Delete(entityToDelete);
    }

    public virtual void Delete(TEntity entityToDelete)
    {
        var entityState = context.Entry(entityToDelete).State;
        if (entityState == EntityState.Deleted || entityState == EntityState.Detached) //already deleted
        {
            return;
        }
        dbSet.Remove(entityToDelete);
    }

    public virtual void Update(TEntity entityToUpdate)
    {
        var entry = context.Entry(entityToUpdate);
        if (entry.State != EntityState.Added)
        {
            entry.State = EntityState.Modified;
        }
    }

    public virtual void Save()
    {
        context.SaveChanges();
    }
}