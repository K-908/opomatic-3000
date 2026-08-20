using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpoMatic3000.Domain.Attempts;
using OpoMatic3000.Domain.Questions;
using OpoMatic3000.Infrastructure.Persistence;

namespace OpoMatic3000.IntegrationTests.Persistence;

public sealed class SqlServerConstraintTests
{
    private const string TestDatabasePrefix = "OpoMatic3000_IntegrationTests_";
    private const string DefaultMasterConnectionString =
        "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True";

    [Fact]
    public Task Topic_name_is_unique_ignoring_case()
    {
        return RunInIsolatedDatabaseAsync(async context =>
        {
            var now = DateTime.UtcNow;
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO [Topics] ([Name], [IsActive], [CreatedAtUtc], [UpdatedAtUtc])
                VALUES ({"Administrative law"}, {true}, {now}, {now})
                """);

            await AssertSqlFailureAsync(
                () => context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO [Topics] ([Name], [IsActive], [CreatedAtUtc], [UpdatedAtUtc])
                    VALUES ({"ADMINISTRATIVE LAW"}, {true}, {now}, {now})
                    """),
                2601,
                2627);
        });
    }

    [Fact]
    public Task Historical_question_requires_a_topic_selected_for_the_attempt()
    {
        return RunInIsolatedDatabaseAsync(async context =>
        {
            var now = DateTime.UtcNow;
            var selectedTopic = CreateTopic("Selected topic", now);
            var otherTopic = CreateTopic("Other topic", now);
            var otherQuestion = Assert.Single(otherTopic.Questions);
            context.AddRange(selectedTopic, otherTopic);
            await context.SaveChangesAsync();

            var attempt = new TestAttempt(Guid.NewGuid(), now, 1, 0, 0, 1, 0m);
            attempt.AddTopic(selectedTopic);
            context.Add(attempt);
            await context.SaveChangesAsync();

            await AssertSqlFailureAsync(
                () => context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO [TestAttemptQuestions]
                        ([TestAttemptId], [OriginalQuestionId], [OriginalTopicId], [DisplayOrder],
                         [StatementSnapshot], [TopicNameSnapshot], [Result])
                    VALUES
                        ({attempt.Id}, {otherQuestion.Id}, {otherTopic.Id}, {1},
                         {otherQuestion.Statement}, {otherTopic.Name}, {(byte)QuestionResult.Unanswered})
                    """),
                547);
        });
    }

    [Fact]
    public Task Option_position_must_be_between_one_and_four()
    {
        return RunInIsolatedDatabaseAsync(async context =>
        {
            var topic = CreateTopic("Topic", DateTime.UtcNow);
            var question = Assert.Single(topic.Questions);
            context.Add(topic);
            await context.SaveChangesAsync();

            await AssertSqlFailureAsync(
                () => context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO [QuestionOptions] ([QuestionId], [Text], [Position], [IsCorrect])
                    VALUES ({question.Id}, {"Invalid option"}, {(byte)5}, {false})
                    """),
                547);
        });
    }

    [Fact]
    public Task Submission_id_must_be_unique()
    {
        return RunInIsolatedDatabaseAsync(async context =>
        {
            var submissionId = Guid.NewGuid();
            var completedAtUtc = DateTime.UtcNow;
            await InsertAttemptAsync(context, submissionId, completedAtUtc, 1, 1, 0, 0, 10m);

            await AssertSqlFailureAsync(
                () => InsertAttemptAsync(context, submissionId, completedAtUtc, 1, 0, 0, 1, 0m),
                2601,
                2627);
        });
    }

    [Fact]
    public Task Attempt_counters_and_score_must_be_valid()
    {
        return RunInIsolatedDatabaseAsync(async context =>
        {
            var completedAtUtc = DateTime.UtcNow;

            await AssertSqlFailureAsync(
                () => InsertAttemptAsync(context, Guid.NewGuid(), completedAtUtc, 2, 1, 0, 0, 5m),
                547);

            await AssertSqlFailureAsync(
                () => context.Database.ExecuteSqlRawAsync("""
                    INSERT INTO [TestAttempts]
                        ([SubmissionId], [CompletedAtUtc], [TotalQuestions], [CorrectCount],
                         [IncorrectCount], [UnansweredCount], [Score], [ScoringRuleVersion])
                    VALUES
                        (NEWID(), SYSUTCDATETIME(), 1, 1, 0, 0,
                         CAST(10.000001 AS decimal(9,6)), 1)
                    """),
                547);
        });
    }

    private static Topic CreateTopic(string name, DateTime now)
    {
        var topic = new Topic(name, now);
        topic.AddQuestion(
            $"Question for {name}",
            Enumerable.Range(1, 4).Select(position =>
                new QuestionOptionDefinition($"Option {position}", (byte)position, position == 1)),
            now);
        return topic;
    }

    private static Task<int> InsertAttemptAsync(
        OpoMatic3000DbContext context,
        Guid submissionId,
        DateTime completedAtUtc,
        int totalQuestions,
        int correctCount,
        int incorrectCount,
        int unansweredCount,
        decimal score)
    {
        return context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO [TestAttempts]
                ([SubmissionId], [CompletedAtUtc], [TotalQuestions], [CorrectCount],
                 [IncorrectCount], [UnansweredCount], [Score], [ScoringRuleVersion])
            VALUES
                ({submissionId}, {completedAtUtc}, {totalQuestions}, {correctCount},
                 {incorrectCount}, {unansweredCount}, {score}, {(short)1})
            """);
    }

    private static async Task AssertSqlFailureAsync(Func<Task> action, params int[] expectedNumbers)
    {
        var exception = await Assert.ThrowsAsync<SqlException>(action);
        Assert.Contains(exception.Number, expectedNumbers);
    }

    private static async Task RunInIsolatedDatabaseAsync(Func<OpoMatic3000DbContext, Task> test)
    {
        var databaseName = $"{TestDatabasePrefix}{Guid.NewGuid():N}";
        ValidateTestDatabaseName(databaseName);

        var masterConnectionString = Environment.GetEnvironmentVariable(
            "OPOMATIC_SQLSERVER_TEST_CONNECTION") ?? DefaultMasterConnectionString;
        var masterBuilder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = "master"
        };
        var databaseBuilder = new SqlConnectionStringBuilder(masterBuilder.ConnectionString)
        {
            InitialCatalog = databaseName
        };

        await CreateDatabaseAsync(masterBuilder.ConnectionString, databaseName);

        try
        {
            var options = new DbContextOptionsBuilder<OpoMatic3000DbContext>()
                .UseSqlServer(databaseBuilder.ConnectionString)
                .Options;
            await using var context = new OpoMatic3000DbContext(options);
            await context.Database.MigrateAsync();
            await test(context);
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await DropDatabaseAsync(masterBuilder.ConnectionString, databaseName);
        }
    }

    private static async Task CreateDatabaseAsync(string masterConnectionString, string databaseName)
    {
        ValidateTestDatabaseName(databaseName);
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseAsync(string masterConnectionString, string databaseName)
    {
        ValidateTestDatabaseName(databaseName);
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
            END
            """;
        await command.ExecuteNonQueryAsync();
    }

    private static void ValidateTestDatabaseName(string databaseName)
    {
        if (!databaseName.StartsWith(TestDatabasePrefix, StringComparison.Ordinal) ||
            databaseName.Length != TestDatabasePrefix.Length + 32 ||
            databaseName[TestDatabasePrefix.Length..].Any(character => !Uri.IsHexDigit(character)))
        {
            throw new InvalidOperationException("Refusing to operate on a database outside the integration-test scope.");
        }
    }
}
