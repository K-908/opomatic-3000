using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using OpoMatic3000.Domain.Attempts;
using OpoMatic3000.Infrastructure.Persistence;

namespace OpoMatic3000.IntegrationTests.Persistence;

public sealed class HistoricalModelTests
{
    private readonly IModel _model;

    public HistoricalModelTests()
    {
        var options = new DbContextOptionsBuilder<OpoMatic3000DbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ModelOnly;Trusted_Connection=True")
            .Options;
        using var context = new OpoMatic3000DbContext(options);
        _model = context.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void Attempt_has_score_precision_submission_uniqueness_and_counter_checks()
    {
        var attempt = _model.FindEntityType(typeof(TestAttempt));

        Assert.NotNull(attempt);
        var score = attempt.FindProperty(nameof(TestAttempt.Score));
        Assert.Equal(9, score?.GetPrecision());
        Assert.Equal(6, score?.GetScale());
        Assert.Contains(attempt.GetIndexes(), index =>
            index.GetDatabaseName() == "UX_TestAttempts_SubmissionId" && index.IsUnique);
        Assert.Contains(attempt.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_TestAttempts_Counts_Total");
        Assert.Contains(attempt.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_TestAttempts_Score");
    }

    [Fact]
    public void Historical_question_is_unique_by_order_and_original_question()
    {
        var question = _model.FindEntityType(typeof(TestAttemptQuestion));

        Assert.NotNull(question);
        Assert.Contains(question.GetIndexes(), index =>
            index.GetDatabaseName() == "UX_TestAttemptQuestions_Attempt_DisplayOrder" && index.IsUnique);
        Assert.Contains(question.GetIndexes(), index =>
            index.GetDatabaseName() == "UX_TestAttemptQuestions_Attempt_OriginalQuestion" && index.IsUnique);

        Assert.Contains(question.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(TestAttemptQuestion.TestAttemptId), nameof(TestAttemptQuestion.OriginalTopicId) }) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(TestAttemptTopic));
    }

    [Fact]
    public void Cascades_are_limited_to_attempt_owned_details()
    {
        var attemptTopic = _model.FindEntityType(typeof(TestAttemptTopic));
        var attemptQuestion = _model.FindEntityType(typeof(TestAttemptQuestion));
        var attemptOption = _model.FindEntityType(typeof(TestAttemptOption));

        Assert.Contains(attemptTopic!.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(TestAttempt) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(attemptQuestion!.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(TestAttempt) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(attemptOption!.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(TestAttemptQuestion) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.DoesNotContain(
            attemptQuestion.GetForeignKeys().Where(foreignKey => foreignKey.DeleteBehavior == DeleteBehavior.Cascade),
            foreignKey => foreignKey.PrincipalEntityType.ClrType != typeof(TestAttempt));
    }
}
