using Microsoft.EntityFrameworkCore;
using OpoMatic3000.Domain.Attempts;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Infrastructure.Persistence;

public sealed class OpoMatic3000DbContext(DbContextOptions<OpoMatic3000DbContext> options)
    : DbContext(options)
{
    public DbSet<Topic> Topics => Set<Topic>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();

    public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();

    public DbSet<TestAttemptTopic> TestAttemptTopics => Set<TestAttemptTopic>();

    public DbSet<TestAttemptQuestion> TestAttemptQuestions => Set<TestAttemptQuestion>();

    public DbSet<TestAttemptOption> TestAttemptOptions => Set<TestAttemptOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpoMatic3000DbContext).Assembly);
    }
}
