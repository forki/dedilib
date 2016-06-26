using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace DediLib.Data
{
    public static class QueryResultCompiler
    {
        public static Func<IDataReader, List<TU>> CompileDataReaderToObject<TU>(IDataReader reader)
            where TU : new()
        {
            if (reader == null) throw new ArgumentNullException("reader");

            var dataReader = Expression.Parameter(typeof(IDataReader), "dataReader");
            var readerType = reader.GetType();
            var customReader = Expression.Variable(readerType, "customReader"); // we need the exact type not the interface
            var list = Expression.Variable(typeof(List<TU>), "list");
            var item = Expression.Variable(typeof(TU), "item");
            var result = Expression.Parameter(typeof(List<TU>), "result");

            var expressions = new List<Expression>();
            var labelLoop = Expression.Label();
            var labelEnd = Expression.Label();

            // check if parameter is "null" and throw ArgumentNullException
            expressions.Add(Expression.IfThen(Expression.Equal(dataReader, Expression.Constant(null)), Expression.Throw(Expression.Constant(new ArgumentNullException()))));
            expressions.Add(Expression.Assign(customReader, Expression.Convert(dataReader, readerType)));
            expressions.Add(Expression.Assign(list, Expression.New(typeof(List<TU>))));

            expressions.Add(Expression.Label(labelLoop));
            expressions.Add(Expression.IfThen(Expression.IsFalse(Expression.Call(dataReader, "Read", null, null)), Expression.Goto(labelEnd)));

            // create new item
            expressions.Add(Expression.Assign(item, Expression.New(typeof(TU))));
            
            // field mapping
            var fields = new Dictionary<string, Type>();
            var fieldIndexes = new Dictionary<string, int>();
            for (var i = reader.FieldCount - 1; i >= 0; i--)
            {
                var name = reader.GetName(i);
                fields[name] = reader.GetFieldType(i);
                fieldIndexes[name] = i;
            }

            foreach (var pi in typeof(TU).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Type type;
                var name = pi.Name;
                if (!fields.TryGetValue(name, out type)) continue;

                var nullable = false;
                var nullableType = Nullable.GetUnderlyingType(pi.PropertyType);
                Type castToType;
                if (!IsAssignableType(type, pi.PropertyType))
                {
                    if (nullableType != null)
                    {
                        if (!IsAssignableType(type, nullableType))
                            throw new ArgumentException(string.Format("Nullable property type in field {0} ({1}) cannot be assigned with value type {2}", name, nullableType, type));
                        nullable = true;
                        castToType = type != nullableType ? pi.PropertyType : null;
                    }
                    else throw new ArgumentException(string.Format("Property type field {0} ({1}) cannot be assigned with value type {2}", name, pi.PropertyType, type));
                }
                else
                {
                    castToType = type != pi.PropertyType ? pi.PropertyType : null;
                }

                var fieldIndexExpression = Expression.Constant(fieldIndexes[name]);
                var property = Expression.PropertyOrField(item, name);

                MethodInfo mi;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean: mi = readerType.GetMethod("GetBoolean", new[] { typeof(int) }); break;
                    case TypeCode.Byte: mi = readerType.GetMethod("GetByte", new[] { typeof(int) }); break;
                    case TypeCode.DateTime: mi = readerType.GetMethod("GetDateTime", new[] { typeof(int) }); break;
                    case TypeCode.Decimal: mi = readerType.GetMethod("GetDecimal", new[] { typeof(int) }); break;
                    case TypeCode.Double: mi = readerType.GetMethod("GetDouble", new[] { typeof(int) }); break;
                    case TypeCode.Int16: mi = readerType.GetMethod("GetInt16", new[] { typeof(int) }); break;
                    case TypeCode.Int32: mi = readerType.GetMethod("GetInt32", new[] { typeof(int) }); break;
                    case TypeCode.Int64: mi = readerType.GetMethod("GetInt64", new[] { typeof(int) }); break;
                    case TypeCode.Single: mi = readerType.GetMethod("GetFloat", new[] { typeof(int) }); break;
                    case TypeCode.String: mi = readerType.GetMethod("GetString", new[] { typeof(int) }); nullable = true; break;
                    case TypeCode.UInt16: mi = readerType.GetMethod("GetInt16", new[] { typeof(int) }); break;
                    case TypeCode.UInt32: mi = readerType.GetMethod("GetInt32", new[] { typeof(int) }); break;
                    case TypeCode.UInt64: mi = readerType.GetMethod("GetInt64", new[] { typeof(int) }); break;
                    case TypeCode.Object:
                        if (type == typeof(Guid)) { mi = readerType.GetMethod("GetGuid", new[] { typeof(int) }); break; }
                        throw new NotSupportedException(string.Format("Data type {0} is not supported", type));
                    default:
                        throw new NotSupportedException(string.Format("Data type {0} is not supported", type));
                }

                // DEBUG:
                //expressions.Add(Expression.Call(typeof (Debug).GetMethod("WriteLine", new[] {typeof (object)}),
                //    Expression.Constant(name)));

                if (nullable)
                {
                    // nullable type
                    var assign = castToType == null
                                     ? Expression.Assign(property, Expression.Convert(Expression.Call(customReader, mi, fieldIndexExpression), pi.PropertyType))
                                     : Expression.Assign(property, Expression.Convert(Expression.Convert(Expression.Call(customReader, mi, fieldIndexExpression), castToType), pi.PropertyType));

                    expressions.Add(Expression.IfThen(Expression.Not(Expression.Call(customReader, readerType.GetMethod("IsDBNull", new[] { typeof(int) }), fieldIndexExpression)), assign));
                }
                else
                {
                    // not nullable type
                    var assign = castToType == null
                                     ? Expression.Assign(property, Expression.Call(customReader, mi, fieldIndexExpression))
                                     : Expression.Assign(property, Expression.Convert(Expression.Call(customReader, mi, fieldIndexExpression), castToType));

                    expressions.Add(assign);
                }
            }

            // add item to list
            expressions.Add(Expression.Call(list, "Add", null, item));

            expressions.Add(Expression.Goto(labelLoop));

            expressions.Add(Expression.Label(labelEnd));
            expressions.Add(Expression.Assign(result, list));

            return Expression.Lambda<Func<IDataReader, List<TU>>>(
                Expression.Block(new[] { customReader, list, item, result },
                expressions), dataReader).Compile();
        }

        private static TypeCode GetUnsignedTypeCode(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Int64:
                    return TypeCode.UInt64;
                case TypeCode.Int32:
                    return TypeCode.UInt32;
                case TypeCode.Int16:
                    return TypeCode.UInt16;
                case TypeCode.SByte:
                    return TypeCode.Byte;
                default:
                    return typeCode;
            }
        }

        private static bool IsAssignableType(Type valueType, Type targetType)
        {
            if (valueType == targetType) return true;
            switch (GetUnsignedTypeCode(targetType))
            {
                case TypeCode.UInt64:
                    switch (GetUnsignedTypeCode(valueType))
                    {
                        case TypeCode.UInt64:
                            return true;
                        case TypeCode.UInt32:
                            return true;
                        case TypeCode.UInt16:
                            return true;
                        case TypeCode.Byte:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch (GetUnsignedTypeCode(valueType))
                    {
                        case TypeCode.UInt32:
                            return true;
                        case TypeCode.UInt16:
                            return true;
                        case TypeCode.Byte:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch (GetUnsignedTypeCode(valueType))
                    {
                        case TypeCode.UInt16:
                            return true;
                        case TypeCode.Byte:
                            return true;
                    }
                    return false;
                case TypeCode.Byte:
                    switch (GetUnsignedTypeCode(valueType))
                    {
                        case TypeCode.Byte:
                            return true;
                    }
                    return false;
            }
            return false;
        }
    }
}
