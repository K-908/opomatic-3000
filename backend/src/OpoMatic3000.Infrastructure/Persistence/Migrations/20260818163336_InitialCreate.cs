using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpoMatic3000.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    IncorrectCount = table.Column<int>(type: "int", nullable: false),
                    UnansweredCount = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    ScoringRuleVersion = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttempts", x => x.Id);
                    table.CheckConstraint("CK_TestAttempts_Counts_NonNegative", "[CorrectCount] >= 0 AND [IncorrectCount] >= 0 AND [UnansweredCount] >= 0");
                    table.CheckConstraint("CK_TestAttempts_Counts_Total", "[CorrectCount] + [IncorrectCount] + [UnansweredCount] = [TotalQuestions]");
                    table.CheckConstraint("CK_TestAttempts_Score", "[Score] BETWEEN -2.5 AND 10");
                    table.CheckConstraint("CK_TestAttempts_ScoringRuleVersion", "[ScoringRuleVersion] > 0");
                    table.CheckConstraint("CK_TestAttempts_TotalQuestions", "[TotalQuestions] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "Latin1_General_100_CI_AS"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.CheckConstraint("CK_Topics_Name_Trimmed", "[Name] = LTRIM(RTRIM([Name])) AND LEN([Name]) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Statement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestAttemptTopics",
                columns: table => new
                {
                    TestAttemptId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalTopicId = table.Column<int>(type: "int", nullable: false),
                    TopicNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttemptTopics", x => new { x.TestAttemptId, x.OriginalTopicId });
                    table.ForeignKey(
                        name: "FK_TestAttemptTopics_TestAttempts_TestAttemptId",
                        column: x => x.TestAttemptId,
                        principalTable: "TestAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAttemptTopics_Topics_OriginalTopicId",
                        column: x => x.OriginalTopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Position = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.CheckConstraint("CK_QuestionOptions_Position", "[Position] BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "FK_QuestionOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestAttemptQuestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestAttemptId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalQuestionId = table.Column<int>(type: "int", nullable: false),
                    OriginalTopicId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    StatementSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopicNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Result = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttemptQuestions", x => x.Id);
                    table.CheckConstraint("CK_TestAttemptQuestions_DisplayOrder", "[DisplayOrder] > 0");
                    table.CheckConstraint("CK_TestAttemptQuestions_Result", "[Result] BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_TestAttemptQuestions_Questions_OriginalQuestionId",
                        column: x => x.OriginalQuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestAttemptQuestions_TestAttemptTopics_TestAttemptId_OriginalTopicId",
                        columns: x => new { x.TestAttemptId, x.OriginalTopicId },
                        principalTable: "TestAttemptTopics",
                        principalColumns: new[] { "TestAttemptId", "OriginalTopicId" });
                    table.ForeignKey(
                        name: "FK_TestAttemptQuestions_TestAttempts_TestAttemptId",
                        column: x => x.TestAttemptId,
                        principalTable: "TestAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAttemptQuestions_Topics_OriginalTopicId",
                        column: x => x.OriginalTopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestAttemptOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestAttemptQuestionId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalOptionId = table.Column<int>(type: "int", nullable: false),
                    TextSnapshot = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttemptOptions", x => x.Id);
                    table.CheckConstraint("CK_TestAttemptOptions_DisplayOrder", "[DisplayOrder] BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "FK_TestAttemptOptions_QuestionOptions_OriginalOptionId",
                        column: x => x.OriginalOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestAttemptOptions_TestAttemptQuestions_TestAttemptQuestionId",
                        column: x => x.TestAttemptQuestionId,
                        principalTable: "TestAttemptQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_QuestionOptions_QuestionId_Position",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TopicId_IsActive",
                table: "Questions",
                columns: new[] { "TopicId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TestAttemptOptions_OriginalOptionId",
                table: "TestAttemptOptions",
                column: "OriginalOptionId");

            migrationBuilder.CreateIndex(
                name: "UX_TestAttemptOptions_Question_DisplayOrder",
                table: "TestAttemptOptions",
                columns: new[] { "TestAttemptQuestionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TestAttemptOptions_Question_OriginalOption",
                table: "TestAttemptOptions",
                columns: new[] { "TestAttemptQuestionId", "OriginalOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestAttemptQuestions_OriginalQuestionId",
                table: "TestAttemptQuestions",
                column: "OriginalQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttemptQuestions_OriginalTopicId",
                table: "TestAttemptQuestions",
                column: "OriginalTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttemptQuestions_TestAttemptId_OriginalTopicId",
                table: "TestAttemptQuestions",
                columns: new[] { "TestAttemptId", "OriginalTopicId" });

            migrationBuilder.CreateIndex(
                name: "UX_TestAttemptQuestions_Attempt_DisplayOrder",
                table: "TestAttemptQuestions",
                columns: new[] { "TestAttemptId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TestAttemptQuestions_Attempt_OriginalQuestion",
                table: "TestAttemptQuestions",
                columns: new[] { "TestAttemptId", "OriginalQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_CompletedAtUtc_DESC",
                table: "TestAttempts",
                column: "CompletedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "UX_TestAttempts_SubmissionId",
                table: "TestAttempts",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestAttemptTopics_OriginalTopicId",
                table: "TestAttemptTopics",
                column: "OriginalTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_IsActive",
                table: "Topics",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "UX_Topics_Name",
                table: "Topics",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestAttemptOptions");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "TestAttemptQuestions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TestAttemptTopics");

            migrationBuilder.DropTable(
                name: "TestAttempts");

            migrationBuilder.DropTable(
                name: "Topics");
        }
    }
}
