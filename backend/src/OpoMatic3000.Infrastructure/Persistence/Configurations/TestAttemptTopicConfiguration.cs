using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Attempts;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class TestAttemptTopicConfiguration : IEntityTypeConfiguration<TestAttemptTopic>
{
    public void Configure(EntityTypeBuilder<TestAttemptTopic> builder)
    {
        builder.ToTable("TestAttemptTopics");
        builder.HasKey(topic => new { topic.TestAttemptId, topic.OriginalTopicId });

        builder.Property(topic => topic.TopicNameSnapshot)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasOne(topic => topic.TestAttempt)
            .WithMany(attempt => attempt.Topics)
            .HasForeignKey(topic => topic.TestAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(topic => topic.OriginalTopic)
            .WithMany()
            .HasForeignKey(topic => topic.OriginalTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
