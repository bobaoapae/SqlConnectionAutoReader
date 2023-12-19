using Microsoft.Data.SqlClient;
using SqlConnectionAutoReader;
using SqlConnectionAutoReader.Test;

Console.WriteLine("");
var fakeSqlConnection = new SqlConnection();
var fakeProcedureName = "fakeProcedureName";
var fakeParametersAction = new Action<SqlParameterCollection>(collection => { });
await fakeSqlConnection.ExecuteProcedureAsync<ExampleResult>(fakeProcedureName, fakeParametersAction);
await fakeSqlConnection.ExecuteProcedureAllAsync<ExampleResult>(fakeProcedureName, fakeParametersAction);