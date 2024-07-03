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
        Expression<Func<TEntity, object>> includePropertyExpression = null)
    {

        IQueryable<TEntity> query = dbSet;

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
        //if (entityToDelete is IProxyTargetAccessor proxy)
        //{
        //    entityToDelete = (TEntity)proxy.DynProxyGetTarget();
        //}

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

    //public virtual void Attach(TEntity entityToAttach)
    //{
    //    dbSet.Attach(entityToAttach);
    //}

    //public virtual void Detach(TEntity entityToDetach)
    //{
    //    context.Entry(entityToDetach).State = EntityState.Detached;
    //}

    public void DetachEntities(params TEntity[] entities)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            context.Entry(entity).State = EntityState.Detached;
        }
    }

    public void AttachEntities(params TEntity[] entities)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
        }
    }

    public void DetachAllEntities()
    {
        var changedEntriesCopy = context.ChangeTracker.Entries<TEntity>()
            //.Where(e => e.State == EntityState.Added ||
            //            e.State == EntityState.Modified ||
            //            e.State == EntityState.Deleted)
            .ToList();

        for (int i = 0; i < changedEntriesCopy.Count; i++)
        {
            var entry = changedEntriesCopy[i];
            entry.State = EntityState.Detached;
        }
    }

    public virtual void Save()
    {
        context.SaveChanges();
    }
}