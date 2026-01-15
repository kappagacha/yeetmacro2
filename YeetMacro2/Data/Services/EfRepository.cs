using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using DbUpdateConcurrencyException = Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;

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

    public virtual void Update(TEntity entityToUpdate, Expression<Func<TEntity, object>> updateReferenceExpression = null)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var primaryKey = entityType?.FindPrimaryKey();
        var entry = context.Entry(entityToUpdate);

        // Log entity info for debugging
        var entityInfo = new System.Text.StringBuilder();
        entityInfo.Append("Updating " + typeof(TEntity).Name);

        var keyValues = new List<object>();
        if (primaryKey != null)
        {
            foreach (var property in primaryKey.Properties)
            {
                var value = property.GetGetter().GetClrValue(entityToUpdate);
                keyValues.Add(value);
                entityInfo.Append($", {property.Name}={value}");
            }
        }

        entityInfo.Append($", State={entry.State}");

        if (entry.State == EntityState.Detached)
        {
            if (primaryKey != null)
            {
                var trackedEntity = dbSet.Find(keyValues.ToArray());
                if (trackedEntity != null)
                {
                    var trackedEntry = context.Entry(trackedEntity);
                    entityInfo.Append($", Detached tracked entity (State={trackedEntry.State}, ReferenceEquals={ReferenceEquals(trackedEntity, entityToUpdate)})");
                    System.Diagnostics.Debug.WriteLine(entityInfo.ToString());
                    trackedEntry.State = EntityState.Detached;
                }
            }

            context.Attach(entityToUpdate);
            entry = context.Entry(entityToUpdate);
        }

        if (entry.State != EntityState.Added)
        {
            entry.State = EntityState.Modified;
        }

        if (updateReferenceExpression is not null)
        {
            var updateReferenceValue = updateReferenceExpression.Compile().Invoke(entityToUpdate);
            var updateRefrenceEntry = context.Entry(updateReferenceValue);
            updateRefrenceEntry.State = EntityState.Modified;
        }

        System.Diagnostics.Debug.WriteLine(entityInfo.ToString());
    }

    public virtual void Save()
    {
        try
        {
            context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var details = new System.Text.StringBuilder();
            details.AppendLine("DbUpdateConcurrencyException for " + typeof(TEntity).Name + ":");

            foreach (var entry in ex.Entries)
            {
                details.AppendLine("  Entity: " + entry.Entity.GetType().Name);
                details.AppendLine("  State: " + entry.State);

                // Get primary key info
                var primaryKey = entry.Metadata.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var keyParts = new System.Collections.Generic.List<string>();
                    foreach (var pkProp in primaryKey.Properties)
                    {
                        var propEntry = entry.Properties.FirstOrDefault(p => p.Metadata.Name == pkProp.Name);
                        if (propEntry != null)
                        {
                            keyParts.Add(pkProp.Name + "=" + propEntry.CurrentValue);
                        }
                    }
                    details.AppendLine("  PrimaryKey: " + string.Join(", ", keyParts));
                }

                // Get database values
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry != null)
                {
                    details.AppendLine("  Database values:");
                    foreach (var prop in entry.Properties)
                    {
                        var dbValue = databaseEntry.GetValue<object>(prop.Metadata.Name);
                        details.AppendLine("    " + prop.Metadata.Name + "=" + dbValue);
                    }
                }

                details.AppendLine("  Current values:");
                foreach (var prop in entry.Properties)
                {
                    details.AppendLine("    " + prop.Metadata.Name + "=" + prop.CurrentValue);
                }
            }

            var newEx = new InvalidOperationException(details.ToString(), ex);
            System.Diagnostics.Debug.WriteLine(details.ToString());
            throw newEx;
        }
    }
}