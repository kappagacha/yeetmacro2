using System.Collections.Concurrent;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace YeetMacro2.Data.Services;
public static class ReflectionHelper
{
    private static FieldInfoCollection _fieldInfoCollection = new FieldInfoCollection();
    private static PropertyInfoCollection _propertyInfoCollection = new PropertyInfoCollection();
    private static MethodInfoCollection _methodInfoCollection = new MethodInfoCollection();
    private static TypeConverterCache _typeConverterCache = new TypeConverterCache();
    //Usage: ReflectionHelper.FieldInfoCollection[type][field] instead of type.GetField(field)
    public static FieldInfoCollection FieldInfoCollection => _fieldInfoCollection;
    //Usage: ReflectionHelper.PropertyInfoCollection[type][property] instead of type.GetProperty(property)  
    public static PropertyInfoCollection PropertyInfoCollection => _propertyInfoCollection;
    //Usage: ReflectionHelper.MethodInfoCollection[type][method] instead of type.GetMethod(Method)
    //To properly cache MethodInfo, method subKey needs to be the same reference (this key is of type object; currently only hanling string and object[2])
    public static MethodInfoCollection MethodInfoCollection => _methodInfoCollection;
    //Usage: ReflectionHelper.TypeConverterCache[type] instead of TypeDescriptor.GetConverter(type)
    public static TypeConverterCache TypeConverterCache => _typeConverterCache;

    //https://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
    public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
        Expression<Func<TSource, TProperty>> propertyLambda)
    {
        Type type = typeof(TSource);

        MemberExpression member = propertyLambda.Body as MemberExpression;
        if (member == null)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a method, not a property.",
                propertyLambda.ToString()));

        PropertyInfo propInfo = member.Member as PropertyInfo;
        if (propInfo == null)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a field, not a property.",
                propertyLambda.ToString()));

        //if (type != propInfo.ReflectedType &&
        //    !type.IsSubclassOf(propInfo.ReflectedType))
        //    throw new ArgumentException(string.Format(
        //        "Expression '{0}' refers to a property that is not from type {1}.",
        //        propertyLambda.ToString(),
        //        type));

        return propInfo;
    }

    public static List<PropertyInfo> GetMultiPropertyInfo<TSource, TProperty>(
        Expression<Func<TSource, TProperty>> propertyLambda)
    {
        Type type = typeof(TSource);

        List<PropertyInfo> properties = new List<PropertyInfo>();
        //https://github.com/dotnet/efcore/blob/03c1ba5275b11963ee1d05ad388ded2fda861c1b/src/EFCore/Extensions/Internal/ExpressionExtensions.cs#L73
        //Got clue from the way EF Core does it 
        NewExpression exp = propertyLambda.Body as NewExpression;

        if (exp == null && propertyLambda.Body is MemberExpression)
        {
            properties.Add(GetPropertyInfo(propertyLambda));
        }
        else
        {
            if (!PropertyInfoCollection[type].IsLoaded)
            {
                PropertyInfoCollection[type].Load();
            }

            foreach (MemberInfo member in exp.Members)
            {
                if (PropertyInfoCollection[type].ContainsKey(member.Name))
                {
                    properties.Add(PropertyInfoCollection[type][member.Name]);
                }
            }
        }

        return properties;
    }
}

#region TypeConverterCache
public class TypeConverterCache : ReflectionCache<Type, TypeConverter>
{
    public override TypeConverter this[Type type]
    {
        get
        {
            if (!_cache.ContainsKey(type))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                _cache.TryAdd(type, converter);
            }

            return _cache[type];
        }
    }
}
#endregion TypeConverterCache

#region FieldInfoCollection
public class FieldInfoCollection : ReflectionCache<Type, IReflectionCache<String, FieldInfo>>
{
    public override IReflectionCache<String, FieldInfo> this[Type type]
    {
        get
        {
            if (!_cache.ContainsKey(type))
            {
                Type genericTemplate = typeof(FieldInfoCache<>);
                Type[] typeArgs = { type };
                Type genericType = genericTemplate.MakeGenericType(typeArgs);
                IReflectionCache<String, FieldInfo> subCache = (IReflectionCache<String, FieldInfo>)Activator.CreateInstance(genericType);
                _cache.TryAdd(type, subCache);
            }

            return _cache[type];
        }
    }
}
#endregion FieldInfoCollection

#region FieldInfoCache<T>
public class FieldInfoCache<T> : ReflectionCache<String, FieldInfo>
{
    public override FieldInfo this[string field]
    {
        get
        {
            if (!_cache.ContainsKey(field))
            {
                const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
                Type t = typeof(T);
                FieldInfo fieldInfo;

                //https://stackoverflow.com/questions/6961781/reflecting-a-private-field-from-a-base-class
                while ((fieldInfo = t.GetField(field, bf)) == null && (t = t.BaseType) != null) ;

                _cache.TryAdd(field, fieldInfo);
            }
            return _cache[field];
        }
    }
}
#endregion FieldInfoCache<T>

#region iReflectionCache
public interface IReflectionCache<TKey, TValue> : IEnumerable<TValue>
{
    TValue this[TKey key] { get; }
    Boolean IsLoaded { get; }
    void Load();
    Boolean ContainsKey(TKey key);
}
#endregion iReflectionCache

#region ReflectionCache
public abstract class ReflectionCache<TKey, TValue> : IReflectionCache<TKey, TValue>
{
    protected ConcurrentDictionary<TKey, TValue> _cache = new ConcurrentDictionary<TKey, TValue>();
    public Boolean IsLoaded { get; protected set; } = false;

    public abstract TValue this[TKey key] { get; }

    #region Initialization
    public virtual void Load()
    {
    }
    #endregion Initialization

    #region IEnumerator
    public IEnumerator<TValue> GetEnumerator()
    {
        foreach (KeyValuePair<TKey, TValue> kvp in _cache)
        {
            yield return kvp.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion IEnumerator

    public Boolean ContainsKey(TKey key)
    {
        return _cache.ContainsKey(key);
    }
}
#endregion ReflectionCache

#region PropertyInfoCollection
public class PropertyInfoCollection : ReflectionCache<Type, IReflectionCache<String, PropertyInfo>>
{
    public override IReflectionCache<String, PropertyInfo> this[Type type]
    {
        get
        {
            if (!_cache.ContainsKey(type))
            {
                Type genericTemplate = typeof(PropertyInfoCache<>);
                Type[] typeArgs = { type };
                Type genericType = genericTemplate.MakeGenericType(typeArgs);
                IReflectionCache<String, PropertyInfo> subCache = (IReflectionCache<String, PropertyInfo>)Activator.CreateInstance(genericType);
                _cache.TryAdd(type, subCache);
            }

            return _cache[type];
        }
    }
}
#endregion PropertyInfoCollection

#region PropertyInfoCache<T>
public class PropertyInfoCache<T> : ReflectionCache<String, PropertyInfo>
{
    public override PropertyInfo this[string key]
    {
        get
        {
            if (!_cache.ContainsKey(key))
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(key);
                _cache.TryAdd(key, propertyInfo);
            }
            return _cache[key];
        }
    }

    public override void Load()
    {
        foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!_cache.ContainsKey(propertyInfo.Name))
            {
                _cache.TryAdd(propertyInfo.Name, propertyInfo);
            }
        }
        IsLoaded = true;
    }
}
#endregion PropertyInfoCache<T>

#region MethodInfoCollection
public class MethodInfoCollection : ReflectionCache<Type, IReflectionCache<Object, MethodInfo>>
{
    public override IReflectionCache<Object, MethodInfo> this[Type type]
    {
        get
        {
            if (!_cache.ContainsKey(type))
            {
                Type genericTemplate = typeof(MethodInfoCache<>);
                Type[] typeArgs = { type };
                Type genericType = genericTemplate.MakeGenericType(typeArgs);
                IReflectionCache<Object, MethodInfo> subCache = (IReflectionCache<Object, MethodInfo>)Activator.CreateInstance(genericType);
                _cache.TryAdd(type, subCache);
            }

            return _cache[type];
        }
    }
}
#endregion MethodInfoCollection

#region MethodInfoCache<T>
public class MethodInfoCache<T> : ReflectionCache<object, MethodInfo>
{
    public override MethodInfo this[object objKey]
    {
        get
        {
            if (!_cache.ContainsKey(objKey))
            {
                MethodInfo methodInfo = null;

                switch (objKey)
                {
                    case String strKey:
                        methodInfo = typeof(T).GetMethod(strKey);
                        break;
                    case Object[] arr:
                        if (arr.Length == 2)
                        {
                            String methodName = arr[0].ToString();
                            Type[] types = (Type[])arr[1];
                            methodInfo = typeof(T).GetMethod(methodName, types);
                        }
                        break;
                }

                _cache.TryAdd(objKey, methodInfo);
            }
            return _cache[objKey];
        }
    }
}
#endregion MethodInfoCache<T>
