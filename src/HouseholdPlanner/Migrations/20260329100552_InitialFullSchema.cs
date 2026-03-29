using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HouseholdPlanner.Migrations
{
    /// <inheritdoc />
    public partial class InitialFullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "meal_tags",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TagType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meal_plans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MealId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meal_plans_meals_MealId",
                        column: x => x.MealId,
                        principalSchema: "public",
                        principalTable: "meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meal_tag_maps",
                schema: "public",
                columns: table => new
                {
                    MealId = table.Column<int>(type: "integer", nullable: false),
                    MealTagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_tag_maps", x => new { x.MealId, x.MealTagId });
                    table.ForeignKey(
                        name: "FK_meal_tag_maps_meal_tags_MealTagId",
                        column: x => x.MealTagId,
                        principalSchema: "public",
                        principalTable: "meal_tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_meal_tag_maps_meals_MealId",
                        column: x => x.MealId,
                        principalSchema: "public",
                        principalTable: "meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "availability_slots",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartLocalTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndLocalTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_availability_slots_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssigneeId = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tasks_users_AssigneeId",
                        column: x => x.AssigneeId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "subtasks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    PlannerTaskId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subtasks_tasks_PlannerTaskId",
                        column: x => x.PlannerTaskId,
                        principalSchema: "public",
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_schedules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlannerTaskId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartLocalTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    AmountOfTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_schedules_tasks_PlannerTaskId",
                        column: x => x.PlannerTaskId,
                        principalSchema: "public",
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_schedules_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_availability_slots_UserId",
                schema: "public",
                table: "availability_slots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_meal_plans_MealId",
                schema: "public",
                table: "meal_plans",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_meal_tag_maps_MealTagId",
                schema: "public",
                table: "meal_tag_maps",
                column: "MealTagId");

            migrationBuilder.CreateIndex(
                name: "IX_subtasks_PlannerTaskId",
                schema: "public",
                table: "subtasks",
                column: "PlannerTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_task_schedules_PlannerTaskId",
                schema: "public",
                table: "task_schedules",
                column: "PlannerTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_task_schedules_UserId",
                schema: "public",
                table: "task_schedules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_AssigneeId",
                schema: "public",
                table: "tasks",
                column: "AssigneeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_slots",
                schema: "public");

            migrationBuilder.DropTable(
                name: "meal_plans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "meal_tag_maps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "subtasks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "meal_tags",
                schema: "public");

            migrationBuilder.DropTable(
                name: "meals",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tasks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
