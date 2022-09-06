using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;

namespace Groffe.AspNetCore.Indisponibilidade;

public class ChecagemIndisponibilidade
{
    private readonly RequestDelegate _next;

    public ChecagemIndisponibilidade(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var config = (ConfiguracoesIndisponibilidade)httpContext
            .RequestServices.GetService(typeof(ConfiguracoesIndisponibilidade))!;
        var logger = (ILogger<ChecagemIndisponibilidade>)httpContext
            .RequestServices.GetService(typeof(ILogger<ChecagemIndisponibilidade>))!;

        var sqlCommand = new StringBuilder();
        sqlCommand.Append("SELECT ");
        if (config.Database == DBChecagemIndisponibilidade.SqlServer)
            sqlCommand.Append("TOP 1 ");
        sqlCommand.Append("Mensagem FROM Indisponibilidade ");
        sqlCommand.Append("WHERE @DataProcessamento BETWEEN InicioIndisponibilidade ");
        sqlCommand.Append("AND TerminoIndisponibilidade ");
        sqlCommand.Append("ORDER BY InicioIndisponibilidade ");
        if (config.Database == DBChecagemIndisponibilidade.SQLite)
            sqlCommand.Append("LIMIT 1 ");

        using var conexao = CreateConnection(config);
        conexao.Open();            
        var cmd = conexao.CreateCommand();
        cmd.CommandText = sqlCommand.ToString();

        var parameter = cmd.CreateParameter();
        parameter.ParameterName = "@DataProcessamento";
        parameter.DbType = DbType.DateTime;
        parameter.Value = DateTime.Now;
        cmd.Parameters.Add(parameter);

        logger.LogInformation(
            "Analisando se a aplicacao deve ser considerada como indisponivel...");
        string? mensagem = null;
        var reader = cmd.ExecuteReader();
        if (reader.Read())
            mensagem = reader["Mensagem"].ToString()!;

        conexao.Close();

        if (mensagem == null)
        {
            logger.LogInformation("Acesso liberado a aplicacao...");
            await _next(httpContext);
        }
        else
        {
            logger.LogError(
                $"Aplicacao configurada como indisponivel - Mensagem de retorno: {mensagem}");
            httpContext.Response.StatusCode = 403;
            httpContext.Response.ContentType = "application/json";
            
            var status = new
            {
                Codigo = 403,
                Status = "Forbidden",
                Mensagem = mensagem
            };
            
            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(status));
        }
    }

    private DbConnection CreateConnection(ConfiguracoesIndisponibilidade config)
    {
        if (config.Database == DBChecagemIndisponibilidade.SqlServer)
            return new SqlConnection(config.ConnectionString);
        else
            return new SQLiteConnection(config.ConnectionString);
    }
}