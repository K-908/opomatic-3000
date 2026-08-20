using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_QuestionOptions_Position",
                "[Position] BETWEEN 1 AND 4"));

        builder.HasKey(option => option.Id);

        builder.Property(option => option.Text)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(option => option.Position)
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(option => option.IsCorrect)
            .IsRequired();

        builder.HasOne(option => option.Question)
            .WithMany(question => question.Options)
            .HasForeignKey(option => option.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(option => new { option.QuestionId, option.Position })
            .IsUnique()
            .HasDatabaseName("UX_QuestionOptions_QuestionId_Position");
    }
}
