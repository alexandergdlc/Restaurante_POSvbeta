using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteEsquemaSupabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: Removed destructive DropPrimaryKey/DropIndex operations to avoid breaking existing DB constraints.
            // The migration will perform non-destructive type alterations and index additions only.

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "TransaccionesPago",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaPago",
                table: "TransaccionesPago",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                table: "Platos",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaPlatoCategoriaId",
                table: "Platos",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Ordenes",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraInicio",
                table: "Ordenes",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraCierre",
                table: "Ordenes",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioUnitario",
                table: "DetallesOrden",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            // Primary keys preserved as they exist in the remote database. No PK creation performed here.

            // Non-destructive index/foreign key changes left minimal to avoid conflicts with existing objects.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Platos_CategoriasPlato_CategoriaPlatoCategoriaId",
                table: "Platos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransaccionesPago_PagoId",
                table: "TransaccionesPago");

            migrationBuilder.DropIndex(
                name: "IX_Platos_CategoriaId",
                table: "Platos");

            migrationBuilder.DropIndex(
                name: "IX_Platos_CategoriaPlatoCategoriaId",
                table: "Platos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ordenes_OrdenId",
                table: "Ordenes");

            migrationBuilder.DropIndex(
                name: "IX_Ordenes_MesaId",
                table: "Ordenes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mesas_MesaId",
                table: "Mesas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Empleados_EmpleadoId",
                table: "Empleados");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesOrden_DetalleId",
                table: "DetallesOrden");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoriasPlato_CategoriaId",
                table: "CategoriasPlato");

            migrationBuilder.DropColumn(
                name: "CategoriaPlatoCategoriaId",
                table: "Platos");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "TransaccionesPago",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaPago",
                table: "TransaccionesPago",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                table: "Platos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Ordenes",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraInicio",
                table: "Ordenes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraCierre",
                table: "Ordenes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioUnitario",
                table: "DetallesOrden",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransaccionesPago",
                table: "TransaccionesPago",
                column: "PagoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ordenes",
                table: "Ordenes",
                column: "OrdenId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mesas",
                table: "Mesas",
                column: "MesaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Empleados",
                table: "Empleados",
                column: "EmpleadoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesOrden",
                table: "DetallesOrden",
                column: "DetalleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoriasPlato",
                table: "CategoriasPlato",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Platos_CategoriaId_Activo",
                table: "Platos",
                columns: new[] { "CategoriaId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_MesaId_Estado",
                table: "Ordenes",
                columns: new[] { "MesaId", "Estado" });
        }
    }
}
