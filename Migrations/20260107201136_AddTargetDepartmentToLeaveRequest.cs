using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFlowManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetDepartmentToLeaveRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetDepartmentId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_TargetDepartmentId",
                table: "LeaveRequests",
                column: "TargetDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Departments_TargetDepartmentId",
                table: "LeaveRequests",
                column: "TargetDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Departments_TargetDepartmentId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_TargetDepartmentId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "TargetDepartmentId",
                table: "LeaveRequests");
        }
    }
}
