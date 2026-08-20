using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.HasKey(question => question.Id);

        builder.Property(question => question.Statement)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(question => question.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(question => question.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(question => question.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(question => question.Topic)
            .WithMany(topic => topic.Questions)
            .HasForeignKey(question => question.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(question => new { question.TopicId, question.IsActive })
            .HasDatabaseName("IX_Questions_TopicId_IsActive");

        builder.Navigation(question => question.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
