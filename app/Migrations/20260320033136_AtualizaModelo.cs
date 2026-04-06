using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unidade",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20)
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<bool>(
                name: "TemPasta",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<bool>(
                name: "TemDocumento",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "ModificadoPor",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ModificadoEm",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<string>(
                name: "DescricaoCompleta",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 140,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 140)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CriadoEm",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50)
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 0)
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unidade",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<bool>(
                name: "TemPasta",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<bool>(
                name: "TemDocumento",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "ModificadoPor",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ModificadoEm",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<string>(
                name: "DescricaoCompleta",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 140,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 140)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CriadoEm",
                table: "Engenharia_Produtos",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Engenharia_Produtos",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Engenharia_Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
