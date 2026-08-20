using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Infrastructure.Persistence.Configurations;

internal sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_Topics_Name_Trimmed",
                "[Name] = LTRIM(RTRIM([Name])) AND LEN([Name]) > 0"));

        builder.HasKey(topic => topic.Id);

        builder.Property(topic => topic.Name)
            .HasMaxLength(150)
            .UseCollation("Latin1_General_100_CI_AS")
            .IsRequired();

        builder.Property(topic => topic.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(topic => topic.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(topic => topic.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(topic => topic.Name)
            .IsUnique()
            .HasDatabaseName("UX_Topics_Name");

        builder.HasIndex(topic => topic.IsActive)
            .HasDatabaseName("IX_Topics_IsActive");

        builder.Navigation(topic => topic.Questions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
