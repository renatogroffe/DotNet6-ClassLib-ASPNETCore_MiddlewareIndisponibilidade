using FluentMigrator;

namespace Groffe.AspNetCore.Indisponibilidade.Migrations;

[Migration(001)]
public class IndisponibilidadeMigration_v001 : Migration
{
    public override void Up()
    {
        Create.Table("Indisponibilidade")
            .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey()
            .WithColumn("InicioIndisponibilidade").AsDateTime().NotNullable()
            .WithColumn("TerminoIndisponibilidade").AsDateTime().NotNullable()
            .WithColumn("Mensagem").AsAnsiString(1000).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Indisponibilidade");
    }
}