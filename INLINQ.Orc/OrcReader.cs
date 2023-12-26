using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Globalization;


namespace INLINQ.Orc
{
    public class OrcReader
    {
        private readonly Type _type;
        private readonly FileTail _fileTail;
        private readonly bool _ignoreMissingColumns;

        //public static long timeReadLong { get; private set; }
        //public static long timeSetLong { get; private set; }
        //public static long timeStripStreamCollection { get; private set; }
        //public static long timeStreamNext { get; private set; }
        //public static long timeTotal { get; private set; }
        //public static long timeInvestigate { get; private set; }
        //public static long timeInit { get; private set; }

        public OrcReader(Type type, Stream inputStream, bool ignoreMissingColumns = false)
        {
            //Console.WriteLine(Helpers.DebuggerHelper.GetTimeString() + " APACHE ORC");

            _type = type;
            _ignoreMissingColumns = ignoreMissingColumns;
            _fileTail = new FileTail(inputStream);

            if (_fileTail.Footer.Types[0].Kind != Protocol.ColumnTypeKind.Struct)
            {
                throw new InvalidDataException($"The base type must be {nameof(Protocol.ColumnTypeKind.Struct)}");
            }
        }

        private static void SetValues<T, TRow>(PropertyInfo propertyInfo, bool hasPresent, byte[] presentMaps, T[] values, TRow[] objects, ulong count)
        {
            //Action<PocoTable, T> valueSetter = GetValueSetter<PocoTable, T>(propertyInfo);
            FirstByRefAction<TRow, T> valueSetter = GetValueSetterRef<TRow, T>(propertyInfo);

            MethodInfo? setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
            {
                throw new ArgumentException("GetSetMethod is null in member " + propertyInfo.Name);
            }
            if (hasPresent)
            {
                int valueId = 0;
                int mapId = 0;
                byte currentMap = 0;
                for (ulong i = 0; i < count; i++)
                {
                    if (((byte)i & 7) == 0)
                    {
                        currentMap = presentMaps[mapId++];
                    }

                    if (currentMap >= 0x80)
                    {
                        var v = values[valueId++];
                        //setMethod.Invoke(objects[i], new object[] { valueCaster(v) });
                        valueSetter(ref objects[i], v);
                    }
                    currentMap <<= 1;
                }
            }
            else
            {
                for (ulong i = 0; i < count; i++)
                {
                    var v = values[i];
                    //setMethod.Invoke(objects[i], new object[] { valueCaster(v) });
                    valueSetter(ref objects[i], v);
                }
            }

        }


        private static Action<PocoTable, FromT> GetValueSetter<PocoTable, FromT>(PropertyInfo propertyInfo)
        {
            MethodInfo? getSetMethod = propertyInfo.GetSetMethod();
            if (getSetMethod == null)
            {
                throw new ArgumentException("GetSetMethod is null in member " + propertyInfo.Name);
            }

            ParameterExpression? instance = Expression.Parameter(typeof(PocoTable), "instance");
            ParameterExpression? value = Expression.Parameter(typeof(FromT), "value");
            UnaryExpression? valueAsType = Expression.Convert(value, propertyInfo.PropertyType);
            MethodCallExpression? callSetter = Expression.Call(instance, getSetMethod, valueAsType);
            ParameterExpression[]? parameters = new ParameterExpression[] { instance, value };
            return Expression.Lambda<Action<PocoTable, FromT>>(callSetter, parameters).Compile();
        }

        private delegate void FirstByRefAction<T1, T2>(ref T1 arg1, T2 arg2);

        private static FirstByRefAction<PocoTable, FromT> GetValueSetterRef<PocoTable, FromT>(PropertyInfo propertyInfo)
        {
            MethodInfo? getSetMethod = propertyInfo.GetSetMethod();
            if (getSetMethod == null)
            {
                throw new ArgumentException("GetSetMethod is null in member " + propertyInfo.Name);
            }

            ParameterExpression? structByRef = Expression.Parameter(typeof(PocoTable).MakeByRefType(), "structByRef");
            ParameterExpression? value = Expression.Parameter(typeof(FromT), "value");
            UnaryExpression? valueAsType = Expression.Convert(value, propertyInfo.PropertyType);
            MethodCallExpression? callSetter = Expression.Call(structByRef, getSetMethod, valueAsType);
            ParameterExpression[]? parameters = new ParameterExpression[] { structByRef, value };
            return Expression.Lambda<FirstByRefAction<PocoTable, FromT>>(callSetter, parameters).Compile();
        }


        public IEnumerable<T> Read<T>() where T: new()
        {
            List<(PropertyInfo propertyInfo, uint columnId, Protocol.ColumnTypeKind columnType)>? properties = FindColumnsForType(_type, _fileTail.Footer).ToList();
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            long lastStop = 0;
            //timeInit -= lastStop - (lastStop = sw.ElapsedMilliseconds);

            ulong maxNumRows = 0;
            foreach (Stripes.StripeReader? stripe in _fileTail.Stripes)
            {
                if (stripe.NumRows > maxNumRows)
                {
                    maxNumRows = stripe.NumRows;
                }
            }

            //allocate buffer based on max numrows:
            byte[] presentMaps = new byte[(maxNumRows + 7) / 8];
            T[] objects = new T[maxNumRows];

            foreach (Stripes.StripeReader? stripe in _fileTail.Stripes)
            {
                //init result array:
                //lastStop = sw.ElapsedMilliseconds;
                var stripeStart = lastStop;
                for (ulong i = 0; i < stripe.NumRows; i++)
                {
                    objects[i] = Helpers.FastActivator<T>.Create();
                }

                //timeStreamNext -= lastStop - (lastStop = sw.ElapsedMilliseconds);
                Stripes.StripeStreamReaderCollection? stripeStreams = stripe.GetStripeStreamCollection();
                //timeStripStreamCollection -= lastStop - (lastStop = sw.ElapsedMilliseconds);
                foreach (var p in properties)
                {
                    uint columnId = p.columnId;
                    var propertyInfo = p.propertyInfo;
                    bool hasPresent;
                    switch (p.columnType)
                    {
                        case Protocol.ColumnTypeKind.Long:
                        case Protocol.ColumnTypeKind.Int:
                        case Protocol.ColumnTypeKind.Short:
                            //lastStop = sw.ElapsedMilliseconds;
                            long[] longColumn = new long[stripe.NumRows];
                            hasPresent = ColumnTypes.LongReader.ReadAll(stripeStreams, columnId, presentMaps, longColumn);
                            //timeReadLong -= lastStop - (lastStop = sw.ElapsedMilliseconds);
                            SetValues(propertyInfo, hasPresent, presentMaps, longColumn, objects, stripe.NumRows);
                            //timeSetLong -= lastStop - (lastStop = sw.ElapsedMilliseconds);
                            break;
                        case Protocol.ColumnTypeKind.Byte:
                            byte[] byteColumn = new byte[stripe.NumRows];
                            hasPresent = ColumnTypes.ByteReader.ReadAll(stripeStreams, columnId, presentMaps, byteColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, byteColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Boolean:
                            bool[] boolColumn = new bool[stripe.NumRows];
                            hasPresent = ColumnTypes.BooleanReader.ReadAll(stripeStreams, columnId, presentMaps, boolColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, boolColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Float:
                            float[] floatColumn = new float[stripe.NumRows];
                            hasPresent = ColumnTypes.FloatReader.ReadAll(stripeStreams, columnId, presentMaps, floatColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, floatColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Double:
                            double[] doubleColumn = new double[stripe.NumRows];
                            hasPresent = ColumnTypes.DoubleReader.ReadAll(stripeStreams, columnId, presentMaps, doubleColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, doubleColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Binary:
                            byte[][] binaryColumn = new byte[stripe.NumRows][];
                            hasPresent = ColumnTypes.BinaryReader.ReadAll(stripeStreams, columnId, presentMaps, binaryColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, binaryColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Decimal:
                            decimal[] decimalColumn = new decimal[stripe.NumRows];
                            hasPresent = ColumnTypes.DecimalReader.ReadAll(stripeStreams, columnId, presentMaps, decimalColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, decimalColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Timestamp:
                            DateTime[] timestampColumn = new DateTime[stripe.NumRows];
                            hasPresent = ColumnTypes.TimestampReader.ReadAll(stripeStreams, columnId, presentMaps, timestampColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, timestampColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.Date:
                            DateTime[] dateColumn = new DateTime[stripe.NumRows];
                            hasPresent = ColumnTypes.DateReader.ReadAll(stripeStreams, columnId, presentMaps, dateColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, dateColumn, objects, stripe.NumRows);
                            break;
                        case Protocol.ColumnTypeKind.String:
                            string[] stringColumn = new string[stripe.NumRows];
                            hasPresent = ColumnTypes.StringReader.ReadAll(stripeStreams, columnId, presentMaps, stringColumn);
                            SetValues(propertyInfo, hasPresent, presentMaps, stringColumn, objects, stripe.NumRows);
                            break;
                        default:
                            throw new NotImplementedException($"Column type {p.columnType} is not supported");
                    }
                }

                //timeTotal += sw.ElapsedMilliseconds - stripeStart;

                for (ulong i = 0; i < stripe.NumRows; i++)
                {
                    yield return objects[i];
                }
            }
        }

        
        private IEnumerable<(PropertyInfo propertyInfo, uint columnId, Protocol.ColumnTypeKind columnType)> FindColumnsForType(Type type, Protocol.Footer footer)
        {
            foreach (PropertyInfo? property in GetWritablePublicProperties(type))
            {
                int columnId = footer.Types[0].FieldNames.FindIndex(fn => fn.ToLower(CultureInfo.InvariantCulture) == property.Name.ToLower(CultureInfo.InvariantCulture)) + 1;
                if (columnId == 0)
                {
                    if (_ignoreMissingColumns)
                    {
                        continue;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"'{property.Name}' not found in ORC data");
                    }
                }
                Protocol.ColumnTypeKind columnType = footer.Types[columnId].Kind;
                yield return (property, (uint)columnId, columnType);
            }
        }

        private static IEnumerable<PropertyInfo> GetWritablePublicProperties(Type type)
        {
            return type.GetTypeInfo().DeclaredProperties.Where(p => p.SetMethod != null);
        }

        private static void SetValues<FromT>(PropertyInfo propertyInfo, object[] objects, bool[] present, bool hasPresent, FromT[] values)
        {
            Type? declaringType = propertyInfo.DeclaringType;
            MethodInfo? getSetMethod = propertyInfo.GetSetMethod();
            if (declaringType == null)
            {
                throw new ArgumentException("Type is null for member " + propertyInfo.Name);
            }
            if (getSetMethod == null)
            {
                throw new ArgumentException("GetSetMethod is null in member " + propertyInfo.Name);
            }

            ParameterExpression? instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression? value = Expression.Parameter(typeof(FromT), "value");
            UnaryExpression? valueAsType = Expression.Convert(value, propertyInfo.PropertyType);
            UnaryExpression? instanceAsType = Expression.Convert(instance, declaringType);
            MethodCallExpression? callSetter = Expression.Call(instanceAsType, getSetMethod, valueAsType);
            ParameterExpression[]? parameters = new ParameterExpression[] { instance, value };
            var setter = Expression.Lambda<Action<object, FromT>>(callSetter, parameters).Compile();
            if (hasPresent)
            {
                uint presentIndex = 0;
                uint valueIndex = 0;
                for (uint rowId = 0; rowId < objects.Length; rowId++)
                {
                    if (present[presentIndex++])
                    {
                        setter.Invoke(objects[rowId], values[valueIndex++]);
                    }
                    else
                    {
                        // keep the default value
                    }
                }
            }
            else
            {
                for (uint rowId = 0; rowId < objects.Length; rowId++)
                {
                    setter.Invoke(objects[rowId], values[rowId]);
                }
            }
        }
    }
}
