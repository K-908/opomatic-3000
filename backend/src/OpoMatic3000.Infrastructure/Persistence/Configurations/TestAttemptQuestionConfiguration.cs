using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Attempts;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class TestAttemptQuestionConfiguration : IEntityTypeConfiguration<TestAttemptQuestion>
{
    public void Configure(EntityTypeBuilder<TestAttemptQuestion> builder)
    {
        builder.ToTable("TestAttemptQuestions", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_TestAttemptQuestions_DisplayOrder", "[DisplayOrder] > 0");
            tableBuilder.HasCheckConstraint("CK_TestAttemptQuestions_Result", "[Result] BETWEEN 0 AND 2");
        });

        builder.HasKey(question => question.Id);

        builder.Property(question => question.DisplayOrder).IsRequired();
        builder.Property(question => question.StatementSnapshot).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(question => question.TopicNameSnapshot).HasMaxLength(150).IsRequired();
        builder.Property(question => question.Result).HasConversion<byte>().HasColumnType("tinyint").IsRequired();

        builder.HasOne(question => question.TestAttempt)
            .WithMany(attempt => attempt.Questions)
            .HasForeignKey(question => question.TestAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(question => question.AttemptTopic)
            .WithMany()
            .HasForeignKey(question => new { question.TestAttemptId, question.OriginalTopicId })
            .HasPrincipalKey(topic => new { topic.TestAttemptId, topic.OriginalTopicId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(question => question.OriginalQuestion)
            .WithMany()
            .HasForeignKey(question => question.OriginalQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(question => question.OriginalTopic)
            .WithMany()
            .HasForeignKey(question => question.OriginalTopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(question => new { question.TestAttemptId, question.DisplayOrder })
            .IsUnique()
            .HasDatabaseName("UX_TestAttemptQuestions_Attempt_DisplayOrder");

        builder.HasIndex(question => new { question.TestAttemptId, question.OriginalQuestionId })
            .IsUnique()
            .HasDatabaseName("UX_TestAttemptQuestions_Attempt_OriginalQuestion");

        builder.HasIndex(question => question.OriginalTopicId)
            .HasDatabaseName("IX_TestAttemptQuestions_OriginalTopicId");

        builder.Navigation(question => question.Options).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
