using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Attempts;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class TestAttemptConfiguration : IEntityTypeConfiguration<TestAttempt>
{
    public void Configure(EntityTypeBuilder<TestAttempt> builder)
    {
        builder.ToTable("TestAttempts", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_TestAttempts_TotalQuestions", "[TotalQuestions] > 0");
            tableBuilder.HasCheckConstraint(
                "CK_TestAttempts_Counts_NonNegative",
                "[CorrectCount] >= 0 AND [IncorrectCount] >= 0 AND [UnansweredCount] >= 0");
            tableBuilder.HasCheckConstraint(
                "CK_TestAttempts_Counts_Total",
                "[CorrectCount] + [IncorrectCount] + [UnansweredCount] = [TotalQuestions]");
            tableBuilder.HasCheckConstraint("CK_TestAttempts_Score", "[Score] BETWEEN -2.5 AND 10");
            tableBuilder.HasCheckConstraint("CK_TestAttempts_ScoringRuleVersion", "[ScoringRuleVersion] > 0");
        });

        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.SubmissionId).IsRequired();
        builder.Property(attempt => attempt.CompletedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(attempt => attempt.TotalQuestions).IsRequired();
        builder.Property(attempt => attempt.CorrectCount).IsRequired();
        builder.Property(attempt => attempt.IncorrectCount).IsRequired();
        builder.Property(attempt => attempt.UnansweredCount).IsRequired();
        builder.Property(attempt => attempt.Score).HasPrecision(9, 6).IsRequired();
        builder.Property(attempt => attempt.ScoringRuleVersion).HasDefaultValue((short)1).IsRequired();

        builder.HasIndex(attempt => attempt.SubmissionId)
            .IsUnique()
            .HasDatabaseName("UX_TestAttempts_SubmissionId");

        builder.HasIndex(attempt => attempt.CompletedAtUtc)
            .IsDescending()
            .HasDatabaseName("IX_TestAttempts_CompletedAtUtc_DESC");

        builder.Navigation(attempt => attempt.Topics).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(attempt => attempt.Questions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
