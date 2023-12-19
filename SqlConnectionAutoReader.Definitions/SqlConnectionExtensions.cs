using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SqlConnectionAutoReader;

public static class SqlConnectionExtensions
{
    public static async Task<StoredProcedureResult> ExecuteProcedureAsync(
        this SqlConnection connection,
        string procedureName,
        Action<SqlParameterCollection> parametersAction = null)
    {
        using var command = new SqlCommand(procedureName, connection);
        command.CommandType = CommandType.StoredProcedure;

        parametersAction?.Invoke(command.Parameters);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
            throw new Exception("Can't read procedure header result");

        var spResult = new StoredProcedureResult
        {
            ResultType = (SpResultType)(int)reader[0],
            Code = (int)reader[1]
        };

        return spResult;
    }

    public static async Task<StoredProcedureResult<T>> ExecuteProcedureAsync<T>(
        this SqlConnection connection,
        string procedureName,
        Action<SqlParameterCollection> parametersAction = null) where T : new()
    {
        throw new NotImplementedException("Implemented by SqlConnectionExtensions.Generator");
    }

    public static async Task<StoredProcedureMultipleResult<T>> ExecuteProcedureAllAsync<T>(
        this SqlConnection connection,
        string procedureName,
        Action<SqlParameterCollection> parametersAction = null) where T : new()
    {
        throw new NotImplementedException("Implemented by SqlConnectionExtensions.Generator");
    }
}