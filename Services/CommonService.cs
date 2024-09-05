using System.Data.SqlClient;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public class CommonService
{
    private ILogger<CommonService> _logger;
    private IConfiguration _config;

    public CommonService(IConfiguration config, ILogger<CommonService> logger)
    {
        _config = config;
        _logger = logger; 
    }

    public IResult ExecuteCommand(
        string command,
        HttpContext ctx,
        Action<SqlCommand>? AddParameters,
        Func<List<Dictionary<string, object>>, IResult>? PrepareOutput)
    {
        var results = new List<Dictionary<string, object>>();

        var form = ctx.Request.Form;
        string? server = form["server"];
        string? connectionString = _config.GetConnectionString(server ?? "DefaultConnection");

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand(command, conn);
            AddParameters?.Invoke(cmd);

            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        results.Add(row);
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                conn.Close(); 
            }
        }
        return PrepareOutput != null ? PrepareOutput.Invoke(results) : Results.Ok(results);
    }
}