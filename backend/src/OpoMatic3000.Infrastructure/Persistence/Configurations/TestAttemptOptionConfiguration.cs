using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Attempts;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class TestAttemptOptionConfiguration : IEntityTypeConfiguration<TestAttemptOption>
{
    public void Configure(EntityTypeBuilder<TestAttemptOption> builder)
    {
        builder.ToTable("TestAttemptOptions", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_TestAttemptOptions_DisplayOrder",
                "[DisplayOrder] BETWEEN 1 AND 4"));

        builder.HasKey(option => option.Id);

        builder.Property(option => option.TextSnapshot).HasMaxLength(1000).IsRequired();
        builder.Property(option => option.DisplayOrder).HasColumnType("tinyint").IsRequired();
        builder.Property(option => option.IsCorrect).IsRequired();
        builder.Property(option => option.IsSelected).IsRequired();

        builder.HasOne(option => option.TestAttemptQuestion)
            .WithMany(question => question.Options)
            .HasForeignKey(option => option.TestAttemptQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(option => option.OriginalOption)
            .WithMany()
            .HasForeignKey(option => option.OriginalOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(option => new { option.TestAttemptQuestionId, option.DisplayOrder })
            .IsUnique()
            .HasDatabaseName("UX_TestAttemptOptions_Question_DisplayOrder");

        builder.HasIndex(option => new { option.TestAttemptQuestionId, option.OriginalOptionId })
            .IsUnique()
            .HasDatabaseName("UX_TestAttemptOptions_Question_OriginalOption");
    }
}
