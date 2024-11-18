using Dapper;
using Dapper.Contrib.Extensions;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text;

namespace TSL.Base.Platform.DapperExt
{
    public static class DbConnectionExtensions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ExplicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypePropertiesExcludeWriteFalse = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeAllProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        private static void FormatInsertReturnKey<T>(out string cmd, out PropertyInfo keyPropertyInfo)
        {
            Type? type = typeof(T);

            bool isArray = false;
            if (!type.IsArray && type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                isArray = typeInfo.ImplementedInterfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            }

            if (type.IsArray || isArray)
            {
                throw new NotImplementedException("未實作多筆新增！");
            }

            string? tableName = GetTableName(type);
            List<PropertyInfo> allProperties = TypePropertiesCacheExcludeWriteFalse(type);
            List<PropertyInfo> keyProperties = KeyPropertiesCache(type);
            List<PropertyInfo> computedProperties = ComputedPropertiesCache(type);
            List<PropertyInfo> allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties)).ToList();

            keyPropertyInfo = GetKeyAndExplicitKeyPropertyInfo(type);

            if (keyPropertyInfo == null)
            {
                throw new ArgumentException("尚未設定 KeyAttribute");
            }

            StringBuilder? sbCol = new(null);
            StringBuilder? sbParams = new(null);
            for (int i = 0; i < allPropertiesExceptKeyAndComputed.Count; i++)
            {
                string? propertyName = allPropertiesExceptKeyAndComputed[i].Name;
                if (i != 0)
                {
                    sbCol.Append(",");
                    sbParams.Append(",");
                }

                sbCol.Append("[" + propertyName + "]");
                sbParams.Append("@" + propertyName);
            }

            cmd = $"insert into {tableName} ({sbCol}) OUTPUT INSERTED.{keyPropertyInfo.Name} as id values ({sbParams});";
        }

        public static TResult? InsertReturnKey<T, TResult>(this IDbConnection conn, T entity, IDbTransaction? tran = null, int? commandTimeout = null)
            where T : class
        {
            FormatInsertReturnKey<T>(out var cmd, out var keyPropertyInfo);

            var multi = conn.QueryMultiple(cmd, entity, tran, commandTimeout);

            var first = multi.Read().FirstOrDefault();
            if (first == null || first.id == null)
            {
                return default;
            }

            var key = Convert.ChangeType(first.id, typeof(TResult));

            keyPropertyInfo.SetValue(entity, key, null);

            return key;
        }

        public static async Task<TResult> InsertReturnKeyAsync<T, TResult>(this IDbConnection conn, T entity, IDbTransaction tran = null, int? commandTimeout = null)
        {
            FormatInsertReturnKey<T>(out var cmd, out var keyPropertyInfo);

            var multi = await conn.QueryMultipleAsync(cmd, entity, tran, commandTimeout).ConfigureAwait(false);

            var first = multi.Read().FirstOrDefault();
            if (first == null || first.id == null)
            {
                return default;
            }

            var key = Convert.ChangeType(first.id, typeof(TResult));

            keyPropertyInfo.SetValue(entity, key, null);

            return key;
        }

        public static string GetTableName(Type type)
        {
            if (TypeTableName.TryGetValue(type.TypeHandle, out string name))
            {
                return name;
            }

            var tableAtt = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();

            name = tableAtt != null ? tableAtt.Name : type.Name;

            TypeTableName[type.TypeHandle] = name;
            return name;
        }

        private static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(WriteAttribute), false).AsList();
            if (attributes.Count != 1)
            {
                return true;
            }

            var writeAttribute = (WriteAttribute)attributes[0];
            return writeAttribute.Write;
        }

        public static List<PropertyInfo> TypePropertiesCacheExcludeWriteFalse(Type type)
        {
            if (TypePropertiesExcludeWriteFalse.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pis))
            {
                return pis.ToList();
            }

            var properties = TypeAllPropertiesCache(type).Where(IsWriteable).ToArray();
            TypePropertiesExcludeWriteFalse[type.TypeHandle] = properties;
            return properties.ToList();
        }

        public static List<PropertyInfo> TypeAllPropertiesCache(Type type)
        {
            if (TypeAllProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties();
            TypeAllProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        public static List<PropertyInfo> KeyPropertiesCache(Type type)
        {
            if (KeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var allProperties = TypePropertiesCacheExcludeWriteFalse(type);
            var keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        public static List<PropertyInfo> ExplicitKeyPropertiesCache(Type type)
        {
            if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var allProperties = TypePropertiesCacheExcludeWriteFalse(type);
            var keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute)).ToList();

            ExplicitKeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        public static PropertyInfo? GetKeyAndExplicitKeyPropertyInfo(Type type)
        {
            var keyProperties = KeyPropertiesCache(type);

            if (keyProperties.Any() == false)
            {
                keyProperties = ExplicitKeyPropertiesCache(type);

                if (keyProperties.Any() == false)
                {
                    return null;
                }
            }

            return keyProperties.First();
        }

        public static List<PropertyInfo> ComputedPropertiesCache(Type type)
        {
            if (ComputedProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var computedProperties = TypePropertiesCacheExcludeWriteFalse(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();

            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }
    }

}
