using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;

namespace Groffe.AspNetCore.Indisponibilidade;

public static class ChecagemIndisponibilidadeExtensions
{
    public static IServiceCollection ConfigureChecagemIndisponibilidade(
        this IServiceCollection services,
        DBChecagemIndisponibilidade database,
        string connectionString)
    {
        services.AddSingleton(new ConfiguracoesIndisponibilidade()
        {
            Database = database,
            ConnectionString = connectionString
        });

        services.AddFluentMigratorCore()
            .ConfigureRunner(cfg =>
            {
                if (database == DBChecagemIndisponibilidade.SqlServer)
                    cfg.AddSqlServer();
                else
                    cfg.AddSQLite();
                cfg.WithGlobalConnectionString(connectionString)
                   .ScanIn(typeof(ChecagemIndisponibilidadeExtensions).Assembly).For.Migrations();
            }).AddLogging(cfg => cfg.AddFluentMigratorConsole());

        var migrationRunner =
            services.BuildServiceProvider(false).GetService<IMigrationRunner>();
        migrationRunner!.MigrateUp();

        return services;
    }

    public static IApplicationBuilder UseChecagemIndisponibilidade(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ChecagemIndisponibilidade>();
    }
}