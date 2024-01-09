using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.FluentSerialization;
using INLINQ.Orc.Helpers;
using INLINQ.Orc.Infrastructure;
using INLINQ.Orc.Statistics;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace INLINQ.Orc.Stripes
{
    public class StripeWriter<TPoco>
    {
        private readonly Stream _outputStream;
        private readonly Compression.OrcCompressedBufferFactory _bufferFactory;
        private readonly int _strideLength;
        private readonly long _stripeLength;
        private readonly TypedColumnWriterDetails<TPoco>[] _columnWriters;
        private readonly List<Protocol.StripeStatistics> _stripeStats = new();
        private bool _rowAddingCompleted;
        private long _rowsInStripe;
        private long _rowsInFile;
        private long _contentLength;
        private readonly List<Protocol.StripeInformation> _stripeInformations = new();
        public readonly Protocol.ColumnType[] _columnTypes;
        
        public StripeWriter(Stream outputStream, bool shouldAlignNumericValues, double uniqueStringThresholdRatio, int defaultDecimalPrecision, int defaultDecimalScale, Compression.OrcCompressedBufferFactory bufferFactory, int strideLength, long stripeLength, SerializationConfiguration? serializationConfiguration)
        {
            if(strideLength % 8 != 0)
            {
                throw new ArgumentException("Provided stridelength does not support bitstream"); //should be a multiple of 8
            }
            _outputStream = outputStream;
            _bufferFactory = bufferFactory;
            _strideLength = strideLength;
            _stripeLength = stripeLength;
            var createResult = CreateColumnWriters(shouldAlignNumericValues, uniqueStringThresholdRatio, defaultDecimalPrecision, defaultDecimalScale, bufferFactory, strideLength, serializationConfiguration);
            _columnTypes = createResult.Item1;
            _columnWriters = createResult.Item2;
        }

        public void AddRows(IEnumerable<TPoco> rows)
        {
            if (_rowAddingCompleted)
            {
                throw new InvalidOperationException("Row adding has been completed");
            }

            object[] buffers = new object[_columnTypes.Length];
            ulong[][] presentMaps = new ulong[_columnTypes.Length][];
            int currentRowId = 0;
            PropertyInfo?[] propertyInfos = GetPublicPropertiesFromPoco(typeof(TPoco).GetTypeInfo()).ToArray();

            for (int columnId = 1; columnId < _columnTypes.Length; columnId++)
            {
                var propertyInfoType = propertyInfos[columnId-1].PropertyType;
                buffers[columnId-1] = _columnWriters[columnId].GetStrideBuffer();
                bool isNullable = NullableHelper.IsNullable(propertyInfoType);
                presentMaps[columnId-1] = new ulong[isNullable ? (_strideLength + 63) / 64 : 0];
            }

            TPoco[] buffer = new TPoco[_strideLength];
            var enumerator = rows.GetEnumerator();
            bool hasNext = enumerator.MoveNext();
            while(true)
            {
                if (currentRowId >= _strideLength || !hasNext)
                {
                    for (int columnId = 0; columnId < _columnWriters.Length; columnId++)
                    {
                        var columnWriter = _columnWriters[columnId];
                        if (columnId > 0)
                        {
                            var action = TypedColumnWriterDetails<TPoco>.AllFillBuffer[columnId -1];
                            action(buffer, buffers[columnId - 1], presentMaps[columnId - 1], 0, currentRowId);
                            columnWriter.AddBlock(buffers[columnId - 1], presentMaps[columnId - 1], currentRowId);
                        }
                        else
                        {
                            columnWriter.AddBlock(null, null, currentRowId);
                        }
                    }

                    CompleteStride(currentRowId);
                    currentRowId = 0;
                }

                if (hasNext)
                {
                    if(enumerator.Current == null)
                    {
                        throw new ArgumentNullException("element in rows");
                    }
                    buffer[currentRowId++] = enumerator.Current;
                    hasNext = enumerator.MoveNext();
                }
                else
                {
                    break;
                }
            }

            RowAddingCompleted();
        }

        public void RowAddingCompleted()
        {
            if (_rowsInStripe != 0)
            {
                CompleteStripe();
            }

            _contentLength = _outputStream.Position;
            _rowAddingCompleted = true;
        }

        public Protocol.Footer GetFooter()
        {
            return !_rowAddingCompleted
                ? throw new InvalidOperationException("Row adding not completed")
                : new Protocol.Footer
                {
                    ContentLength = (ulong)_contentLength,
                    NumberOfRows = (ulong)_rowsInFile,
                    RowIndexStride = (uint)_strideLength,
                    Stripes = _stripeInformations,
                    Statistics = _columnWriters.Select(c => c.FileStatistics).ToList(),
                    Types = GetColumnTypes().ToList()
                };
        }

        public Protocol.Metadata GetMetadata()
        {
            if (!_rowAddingCompleted)
            {
                throw new InvalidOperationException("Row adding not completed");
            }

            return new Protocol.Metadata
            {
                StripeStats = _stripeStats
            };
        }

        private void CompleteStride(int rowsInStride)
        {
            _rowsInStripe += rowsInStride;

            long totalStripeLength = _columnWriters.Sum(writer => writer.ColumnWriter.CompressedLength);
            if (totalStripeLength > _stripeLength)
            {
                CompleteStripe();
            }
        }

        private void CompleteStripe()
        {
            Protocol.StripeFooter? stripeFooter = new();
            Protocol.StripeStatistics? stripeStats = new();

            //Columns
            foreach (TypedColumnWriterDetails<TPoco>? writer in _columnWriters)
            {
                writer.ColumnWriter.FlushBuffers();
                uint dictionaryLength = (writer.ColumnWriter as ColumnTypes.StringWriter)?.DictionaryLength ?? 0;    //DictionaryLength is only used by StringWriter
                stripeFooter.AddColumn(writer.ColumnWriter.ColumnEncoding, dictionaryLength);
            }

            Protocol.StripeInformation? stripeInformation = new();
            stripeInformation.Offset = (ulong)_outputStream.Position;
            stripeInformation.NumberOfRows = (ulong)_rowsInStripe;

            //Indexes
            foreach (TypedColumnWriterDetails<TPoco>? writer in _columnWriters)
            {
                //Write the index buffer
                Compression.OrcCompressedBuffer? indexBuffer = _bufferFactory.CreateBuffer(Protocol.StreamKind.RowIndex);
                writer.ColumnWriter.Statistics.WriteToBuffer(indexBuffer, i => writer.ColumnWriter.Buffers[i].MustBeIncluded);
                
                indexBuffer.CopyTo(_outputStream);
                
                //Add the index to the footer
                stripeFooter.AddDataStream(writer.ColumnWriter.ColumnId, indexBuffer);

                //Collect summary statistics
                ColumnStatistics? columnStats = new();
                foreach (IStatistics? stats in writer.ColumnWriter.Statistics)
                {
                    stats.FillColumnStatistics(columnStats);
                    stats.FillColumnStatistics(writer.FileStatistics);
                }
                stripeStats.ColStats.Add(columnStats);
            }
            _stripeStats.Add(stripeStats);
            

            stripeInformation.IndexLength = (ulong)_outputStream.Position - stripeInformation.Offset;

            //Data streams
            foreach (TypedColumnWriterDetails<TPoco>? writer in _columnWriters)
            {
                foreach (Compression.OrcCompressedBuffer? buffer in writer.ColumnWriter.Buffers)
                {
                    if (!buffer.MustBeIncluded)
                    {
                        continue;
                    }

                    buffer.CopyTo(_outputStream);
                    stripeFooter.AddDataStream(writer.ColumnWriter.ColumnId, buffer);
                }
            }
            
            stripeInformation.DataLength = (ulong)_outputStream.Position - stripeInformation.IndexLength - stripeInformation.Offset;

            //Footer
            _bufferFactory.SerializeAndCompressTo(_outputStream, stripeFooter, out long footerLength);
            stripeInformation.FooterLength = (ulong)footerLength;

            _stripeInformations.Add(stripeInformation);

            _rowsInFile += _rowsInStripe;
            _rowsInStripe = 0;
            foreach (TypedColumnWriterDetails<TPoco>? writer in _columnWriters)
            {
                writer.ColumnWriter.Reset();
            }

        }

        private static Tuple<Protocol.ColumnType[], TypedColumnWriterDetails<TPoco>[]> CreateColumnWriters(bool shouldAlignNumericValues, double uniqueStringThresholdRatio
            ,int defaultDecimalPrecision, int defaultDecimalScale, Compression.OrcCompressedBufferFactory bufferFactory, int strideLength, SerializationConfiguration? serializationConfiguration)
        {
            Type type = typeof(TPoco);
            PropertyInfo?[] propertyInfos = GetPublicPropertiesFromPoco(type.GetTypeInfo()).ToArray();
            TypedColumnWriterDetails<TPoco>[] columnWriters = new TypedColumnWriterDetails<TPoco>[propertyInfos.Length+1];
            Protocol.ColumnType[] columnTypes = new Protocol.ColumnType[propertyInfos.Length+1];
            var structColumnType = new Protocol.ColumnType()
            {
                Kind = Protocol.ColumnTypeKind.Struct
            };
            columnTypes[0] = structColumnType;

            for (uint columnId = 0; columnId < propertyInfos.Length; columnId++)
            {
                PropertyInfo? propertyInfo = propertyInfos[columnId];
                SerializationPropertyConfiguration? propertyConfiguration = GetPropertyConfiguration(type, propertyInfo, serializationConfiguration);
                if (propertyConfiguration != null && propertyConfiguration.ExcludeFromSerialization)
                {
                    continue;
                }

                Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>> columnTypeAndColumnWriterAndAction = GetColumnWriterDetails(propertyInfo, columnId+1, propertyConfiguration, shouldAlignNumericValues
                    , bufferFactory, strideLength, defaultDecimalPrecision, defaultDecimalScale, uniqueStringThresholdRatio);
                columnTypes[columnId + 1] = columnTypeAndColumnWriterAndAction.Item1; 
                columnWriters[columnId+1]= columnTypeAndColumnWriterAndAction.Item2;
                structColumnType.FieldNames.Add(propertyInfo.Name);
                structColumnType.SubTypes.Add(columnTypeAndColumnWriterAndAction.Item2.ColumnWriter.ColumnId);
            }

            StructWriter columnWriter = new(bufferFactory, 0);
            columnWriters[0]= TypedColumnWriterDetails<TPoco>.Create(columnWriter);      //Add the struct column at the beginning
            return new Tuple<Protocol.ColumnType[], TypedColumnWriterDetails<TPoco>[]>(columnTypes, columnWriters);
        }

        private IEnumerable<Protocol.ColumnType> GetColumnTypes()
        {
            foreach (var columnType in _columnTypes)
            {
                yield return columnType;
            }
        }

        private static IEnumerable<PropertyInfo> GetPublicPropertiesFromPoco(TypeInfo pocoTypeInfo)
        {
            if (pocoTypeInfo.BaseType != null)
            {
                foreach (PropertyInfo? property in GetPublicPropertiesFromPoco(pocoTypeInfo.BaseType.GetTypeInfo()))
                {
                    yield return property;
                }
            }

            foreach (PropertyInfo? property in pocoTypeInfo.DeclaredProperties)
            {
                if (property.GetMethod != null && property.CanRead && property.CanWrite)
                {
                    yield return property;
                }
            }
        }

        private static SerializationPropertyConfiguration? GetPropertyConfiguration(Type objectType, PropertyInfo propertyType, SerializationConfiguration? serializationConfiguration)
        {
            if (serializationConfiguration == null)
            {
                return null;
            }

            if (!serializationConfiguration.Types.TryGetValue(objectType, out ISerializationTypeConfiguration? typeConfiguration))
            {
                return null;
            }

            if (!typeConfiguration.Properties.TryGetValue(propertyType, out SerializationPropertyConfiguration? propertyConfiguration))
            {
                return null;
            }

            return propertyConfiguration;
        }

        private static Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>> GetColumnWriterDetails(PropertyInfo propertyInfo, uint columnId
            ,SerializationPropertyConfiguration? propertyConfiguration, bool shouldAlignNumericValues
            , Compression.OrcCompressedBufferFactory bufferFactory, int strideLength, int defaultDecimalPrecision
            , int defaultDecimalScale, double uniqueStringThresholdRatio)
        {
            Type? propertyType = propertyInfo.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            bool isNullable = Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;

            if (underlyingType == typeof(int))
            {
                LongWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<int>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Int }, columnWriter);
            }

            if (underlyingType == typeof(long))
            {
                LongWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<long>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Long }, columnWriter);
            }

            if (underlyingType == typeof(short))
            {
                LongWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<short>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Short }, columnWriter);
            }

            if (underlyingType == typeof(uint))
            {
                LongWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<uint>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Int }, columnWriter);
            }

            if (underlyingType == typeof(ulong))
            {
                LongWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<ulong>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Long }, columnWriter);
            }

            if (underlyingType == typeof(ushort))
            {
                LongWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<ushort>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Short }, columnWriter);
            }

            if (underlyingType == typeof(byte))
            {
                ByteWriter writer = new(isNullable, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<byte>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Byte }, columnWriter);
            }

            if (underlyingType == typeof(sbyte))
            {
                ByteWriter writer = new(isNullable, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<sbyte>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Byte }, columnWriter);

            }

            if (underlyingType == typeof(bool))
            {
                BooleanWriter writer = new(isNullable, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<bool>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Boolean }, columnWriter);
            }

            if (underlyingType == typeof(float))
            {
                FloatWriter writer = new(isNullable, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<float>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Float }, columnWriter);
            }


            if (underlyingType == typeof(double))
            {
                DoubleWriter writer = new(isNullable, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<double>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Double }, columnWriter);
            }

            if (underlyingType == typeof(byte[]))
            {
                ColumnTypes.BinaryWriter writer = new(shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<byte[]>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Binary }, columnWriter);
            }

            if (underlyingType == typeof(decimal))
            {
                int precision = propertyConfiguration?.DecimalPrecision ?? defaultDecimalPrecision;
                int scale = propertyConfiguration?.DecimalScale ?? defaultDecimalScale;
                DecimalWriter writer = new(isNullable, shouldAlignNumericValues, precision, scale, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<decimal>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Decimal, Precision = (uint)precision, Scale = (uint)scale }, columnWriter);
            }

            if (underlyingType == typeof(DateTime) && propertyConfiguration != null && propertyConfiguration.SerializeAsDate)
            {
                DateWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<DateTime>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Date }, columnWriter);
            }

            if (underlyingType == typeof(DateTime))
            {
                TimestampWriter writer = new(isNullable, shouldAlignNumericValues, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<DateTime>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.Timestamp }, columnWriter);
            }

            if (underlyingType == typeof(string))
            {
                ColumnTypes.StringWriter writer = new(shouldAlignNumericValues, shouldAlignNumericValues, uniqueStringThresholdRatio, strideLength, bufferFactory, columnId);
                var columnWriter = TypedColumnWriterDetails<TPoco>.Create<string>(writer, propertyInfo, strideLength, writer.AddBlock);
                return new Tuple<Protocol.ColumnType, TypedColumnWriterDetails<TPoco>>(new Protocol.ColumnType { Kind = Protocol.ColumnTypeKind.String }, columnWriter);
            }

            throw new NotImplementedException($"Only basic types are supported. Unable to handle type {propertyType}");
        }

    }

    

    internal class TypedColumnWriterDetails<TPoco>
    {
        internal static Action<object, object, ulong[], int, int>[] AllFillBuffer = GetAllFillBufferAndPresentFromStructs();

        public IColumnWriter ColumnWriter { get; }
        public ColumnStatistics FileStatistics { get; } = new ColumnStatistics();

        public Func<object> GetStrideBuffer { get; }

        public Func<object, object, int, int> AddBlock { get; }

        private TypedColumnWriterDetails(IColumnWriter columnWriter, Func<object, object, int, int> addBlock, Func<object> getStrideBuffer)
        {
            ColumnWriter = columnWriter;
            AddBlock = addBlock;
            GetStrideBuffer = getStrideBuffer;
        }

        public static TypedColumnWriterDetails<TPoco> Create(StructWriter columnWriter)
        {
            int addCastBlock(object column, object presentMaps, int count) { return columnWriter.AddBlock(count); }
            object getStrideBuffer() { return null; }

            return new TypedColumnWriterDetails<TPoco>(columnWriter, addCastBlock, getStrideBuffer);
        }

        public static TypedColumnWriterDetails<TPoco> Create<UnderlyingType>(IColumnWriter columnWriter,
            PropertyInfo propertyInfo,int strideLength , Func<ReadOnlyMemory<UnderlyingType>, ReadOnlyMemory<ulong>, int, int> addBlock)
        {
            int addCastBlock(object column, object presentMaps, int count) {
                ReadOnlyMemory<UnderlyingType> columnMemory = column.GetType().IsArray
                    ? new ReadOnlyMemory<UnderlyingType>((UnderlyingType[])column)
                    : (ReadOnlyMemory<UnderlyingType>)column;
                ReadOnlyMemory<ulong> presentMemory = presentMaps.GetType().IsArray
                    ? new ReadOnlyMemory<ulong>((ulong[])presentMaps)
                    : (ReadOnlyMemory<ulong>)presentMaps;
                return addBlock(columnMemory, presentMemory, count); 
            }
            object getStrideBuffer() { return new UnderlyingType[strideLength]; }
            return new TypedColumnWriterDetails<TPoco>(columnWriter, addCastBlock, getStrideBuffer);
        }

        private static Action<object, object, ulong[], int, int>[] GetAllFillBufferAndPresentFromStructs()
        {
            PropertyInfo[] propertyInfos = typeof(TPoco).GetProperties();
            Action<object, object, ulong[], int, int>[] result = new Action<object, object, ulong[], int, int>[propertyInfos.Length];
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];
                bool isNullable = NullableHelper.IsNullable(propertyInfo.PropertyType);
                result[i]= FillBufferAndPresentFromStructs(propertyInfo, isNullable);
            }
            return result;
        }

        private static Action<object, object, ulong[], int, int> FillBufferAndPresentFromStructs(PropertyInfo prop, bool isNullable)
        {
            if (prop.ReflectedType == null)
            {
                throw new ArgumentException("Can't find reflected type");
            }
            Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            //parameters
            ParameterExpression arrayOfPocos = Expression.Parameter(typeof(object), "arrayOfPocos");
            ParameterExpression arrayOfPrimitives = Expression.Parameter(typeof(object), "arrayOfPrimitives");
            ParameterExpression presentMaps = Expression.Parameter(typeof(ulong[]), "presentMaps");
            ParameterExpression fromIndex = Expression.Parameter(typeof(int), "fromIndex");
            ParameterExpression toIndex = Expression.Parameter(typeof(int), "toIndex"); //excluding

            //body
            ParameterExpression castArrayOfPocos = Expression.Variable(typeof(TPoco[]), "castArrayOfPocos");
            ParameterExpression tmpPoco = Expression.Variable(typeof(TPoco), "tmpPoco");
            ParameterExpression castArrayOfPrimitives = Expression.Variable(underlyingType.MakeArrayType(), "castArrayOfPrimitives");
            ParameterExpression currentMap = Expression.Variable(typeof(ulong), "currentMap");
            ParameterExpression currentMapIndex = Expression.Variable(typeof(int), "currentMapIndex");
            ParameterExpression currentFieldIndex = Expression.Variable(typeof(int), "currentFieldIndex");
            ParameterExpression temp = Expression.Variable(prop.PropertyType, "temp");
            LabelTarget label = Expression.Label();
            List<Expression> structIndex = new() { fromIndex };
            List<Expression> fieldIndex = new() { isNullable ? currentFieldIndex : fromIndex };
            List<Expression> mapIndex = new() { currentMapIndex };

            //nullable:
            BlockExpression block;
            if (isNullable)
            {
                block = Expression.Block(
                    new[] { castArrayOfPocos, castArrayOfPrimitives, currentMap, currentMapIndex, temp, currentFieldIndex },
                    Expression.Assign(castArrayOfPocos, Expression.Convert(arrayOfPocos, typeof(TPoco[]))),
                    Expression.Assign(castArrayOfPrimitives, Expression.Convert(arrayOfPrimitives, underlyingType.MakeArrayType())),
                    Expression.Assign(currentMapIndex, Expression.Divide(Expression.Add(fromIndex, Expression.Constant(63)), Expression.Constant(64))),
                    Expression.Assign(currentMap, Expression.ArrayAccess(presentMaps, mapIndex)),
                    Expression.Loop( //loop over all Pocos:
                        Expression.IfThenElse(
                            Expression.LessThan(fromIndex, toIndex),
                                Expression.Block( //loop block for nullable properties:
                                    Expression.LeftShiftAssign(currentMap, Expression.Constant(1)),//currentMap<<=1
                                    Expression.Assign(temp, Expression.Property(Expression.ArrayAccess(castArrayOfPocos, structIndex), prop)),//temp = castArrayOfStructs[fromIndex].field
                                    Expression.IfThen(Expression.NotEqual(temp, Expression.Constant(null)),
                                        Expression.Block(//if temp.HasValue
                                            Expression.Assign(Expression.ArrayAccess(castArrayOfPrimitives, fieldIndex), Expression.Convert(temp, underlyingType)),//castArrayOfPrimitives[fieldIndex] = temp.Value
                                            Expression.PostIncrementAssign(currentFieldIndex), //currentFieldIndex++
                                            Expression.PostIncrementAssign(currentMap)
                                        )
                                    ),//currentMap++ update present map
                                    Expression.PostIncrementAssign(fromIndex),

                                    Expression.IfThen(Expression.Equal(Expression.And(fromIndex, Expression.Constant(63)), Expression.Constant(0)),
                                        Expression.Block( //if fromIndex&63 == 0 save the current map:
                                            Expression.Assign(Expression.ArrayAccess(presentMaps, mapIndex), currentMap), //presentMaps[currentMapIndex]=currentMap
                                            Expression.PostIncrementAssign(currentMapIndex)//,//currentMapIndex++
                                        )
                                    )
                                ),
                            Expression.Break(label)
                        ),
                        label
                    ),
                    Expression.IfThen(Expression.NotEqual(Expression.And(fromIndex, Expression.Constant(63)), Expression.Constant(0)),
                            Expression.Assign(Expression.ArrayAccess(presentMaps, mapIndex), Expression.LeftShift(currentMap, Expression.Subtract(Expression.Constant(64), Expression.And(fromIndex, Expression.Constant(63)))))
                    )
                );
            }
            else
            {
                //not nullable:
                block = Expression.Block(new[] { castArrayOfPocos, castArrayOfPrimitives },
                    Expression.Assign(castArrayOfPocos, Expression.Convert(arrayOfPocos, prop.ReflectedType.MakeArrayType())),
                    Expression.Assign(castArrayOfPrimitives, Expression.Convert(arrayOfPrimitives, underlyingType.MakeArrayType())),
                    Expression.Loop( //loop over all structs:
                        Expression.IfThenElse(
                            Expression.LessThan(fromIndex, toIndex),
                            Expression.Block( //loop block for nonnullable properties:
                        Expression.Assign(Expression.ArrayAccess(castArrayOfPrimitives, structIndex), Expression.Property(Expression.ArrayAccess(castArrayOfPocos, structIndex), prop)),
                        Expression.PostIncrementAssign(fromIndex)),
                            Expression.Break(label)
                        ),
                        label
                    ));
            }
            return (Action<object, object, ulong[]?, int, int>)Expression
                 .Lambda(typeof(Action<object, object, ulong[]?, int, int>), block, arrayOfPocos, arrayOfPrimitives, presentMaps, fromIndex, toIndex)
                 .Compile();
        }

    }

}

