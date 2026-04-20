using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_ArjSys_Tcc.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin_Pessoas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CpfCnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Pessoas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Chave = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_GruposProdutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nivel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    QtdCaracteres = table.Column<int>(type: "INTEGER", nullable: false),
                    PathDocumentos = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_GruposProdutos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    DescricaoCompleta = table.Column<string>(type: "TEXT", nullable: true),
                    Unidade = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Peso = table.Column<decimal>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemPasta = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemDocumento = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admin_Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PessoaId = table.Column<int>(type: "INTEGER", nullable: false),
                    RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InscricaoEstadual = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ContatoComercial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admin_Clientes_Admin_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Admin_Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admin_Funcionarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PessoaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cargo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Setor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Usuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Funcionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admin_Funcionarios_Admin_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Admin_Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_GruposVinculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GrupoPaiId = table.Column<int>(type: "INTEGER", nullable: false),
                    GrupoFilhoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_GruposVinculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engenharia_GruposVinculos_Engenharia_GruposProdutos_GrupoFilhoId",
                        column: x => x.GrupoFilhoId,
                        principalTable: "Engenharia_GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Engenharia_GruposVinculos_Engenharia_GruposProdutos_GrupoPaiId",
                        column: x => x.GrupoPaiId,
                        principalTable: "Engenharia_GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_PathDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GrupoProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ControlarPorPrefixo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_PathDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engenharia_PathDocumentos_Engenharia_GruposProdutos_GrupoProdutoId",
                        column: x => x.GrupoProdutoId,
                        principalTable: "Engenharia_GruposProdutos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Engenharia_EstruturasProdutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoPaiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoFilhoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engenharia_EstruturasProdutos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engenharia_EstruturasProdutos_Engenharia_Produtos_ProdutoFilhoId",
                        column: x => x.ProdutoFilhoId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Engenharia_EstruturasProdutos_Engenharia_Produtos_ProdutoPaiId",
                        column: x => x.ProdutoPaiId,
                        principalTable: "Engenharia_Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comercial_PedidosVenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_PedidosVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_PedidosVenda_Admin_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Admin_Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admin_Permissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Nivel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Permissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admin_Permissoes_Admin_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Admin_Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comercial_NumerosSerie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PedidoVendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoProjeto = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_NumerosSerie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_NumerosSerie_Comercial_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "Comercial_PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comercial_PedidosVendaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoVendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_PedidosVendaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_PedidosVendaItens_Comercial_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "Comercial_PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comercial_PedidoVendaHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoVendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Evento = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    StatusAnterior = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    StatusNovo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Justificativa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: true),
                    ModificadoPor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercial_PedidoVendaHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comercial_PedidoVendaHistorico_Comercial_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "Comercial_PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Clientes_PessoaId",
                table: "Admin_Clientes",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Funcionarios_PessoaId",
                table: "Admin_Funcionarios",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Funcionarios_Usuario",
                table: "Admin_Funcionarios",
                column: "Usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Permissoes_FuncionarioId_Modulo",
                table: "Admin_Permissoes",
                columns: new[] { "FuncionarioId", "Modulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_Codigo",
                table: "Comercial_NumerosSerie",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_NumerosSerie_PedidoVendaId",
                table: "Comercial_NumerosSerie",
                column: "PedidoVendaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVenda_ClienteId",
                table: "Comercial_PedidosVenda",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVenda_Codigo",
                table: "Comercial_PedidosVenda",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidosVendaItens_PedidoVendaId",
                table: "Comercial_PedidosVendaItens",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comercial_PedidoVendaHistorico_PedidoVendaId",
                table: "Comercial_PedidoVendaHistorico",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_Configuracoes_Chave",
                table: "Engenharia_Configuracoes",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_EstruturasProdutos_ProdutoFilhoId",
                table: "Engenharia_EstruturasProdutos",
                column: "ProdutoFilhoId");

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_EstruturasProdutos_ProdutoPaiId_ProdutoFilhoId",
                table: "Engenharia_EstruturasProdutos",
                columns: new[] { "ProdutoPaiId", "ProdutoFilhoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_GruposProdutos_Codigo_Nivel",
                table: "Engenharia_GruposProdutos",
                columns: new[] { "Codigo", "Nivel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_GruposVinculos_GrupoFilhoId",
                table: "Engenharia_GruposVinculos",
                column: "GrupoFilhoId");

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_GruposVinculos_GrupoPaiId_GrupoFilhoId",
                table: "Engenharia_GruposVinculos",
                columns: new[] { "GrupoPaiId", "GrupoFilhoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_PathDocumentos_GrupoProdutoId",
                table: "Engenharia_PathDocumentos",
                column: "GrupoProdutoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engenharia_Produtos_Codigo",
                table: "Engenharia_Produtos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_Permissoes");

            migrationBuilder.DropTable(
                name: "Comercial_NumerosSerie");

            migrationBuilder.DropTable(
                name: "Comercial_PedidosVendaItens");

            migrationBuilder.DropTable(
                name: "Comercial_PedidoVendaHistorico");

            migrationBuilder.DropTable(
                name: "Engenharia_Configuracoes");

            migrationBuilder.DropTable(
                name: "Engenharia_EstruturasProdutos");

            migrationBuilder.DropTable(
                name: "Engenharia_GruposVinculos");

            migrationBuilder.DropTable(
                name: "Engenharia_PathDocumentos");

            migrationBuilder.DropTable(
                name: "Admin_Funcionarios");

            migrationBuilder.DropTable(
                name: "Comercial_PedidosVenda");

            migrationBuilder.DropTable(
                name: "Engenharia_Produtos");

            migrationBuilder.DropTable(
                name: "Engenharia_GruposProdutos");

            migrationBuilder.DropTable(
                name: "Admin_Clientes");

            migrationBuilder.DropTable(
                name: "Admin_Pessoas");
        }
    }
}
