using System.Collections.Generic;

namespace SqlConnectionAutoReader
{
    public class StoredProcedureResult
    {
        public SpResultType ResultType { get; init; }
        public int Code { get; init; }
    }

    public class StoredProcedureResult<T> : StoredProcedureResult
    {
        public T Result { get; init; }
    }

    public class StoredProcedureMultipleResult<T> : StoredProcedureResult
    {
        public List<T> Result { get; init; }
    }
}
