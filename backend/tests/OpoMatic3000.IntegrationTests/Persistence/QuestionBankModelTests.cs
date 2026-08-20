using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using OpoMatic3000.Domain.Questions;
using OpoMatic3000.Infrastructure.Persistence;

namespace OpoMatic3000.IntegrationTests.Persistence;

public sealed class QuestionBankModelTests
{
    private readonly IModel _model;

    public QuestionBankModelTests()
    {
        var options = new DbContextOptionsBuilder<OpoMatic3000DbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ModelOnly;Trusted_Connection=True")
            .Options;
        using var context = new OpoMatic3000DbContext(options);
        _model = context.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void Topic_mapping_has_trim_check_case_insensitive_unique_name_and_active_index()
    {
        var topic = _model.FindEntityType(typeof(Topic));

        Assert.NotNull(topic);
        Assert.Contains(topic.GetCheckConstraints(), constraint => constraint.Name == "CK_Topics_Name_Trimmed");
        Assert.Contains(topic.GetIndexes(), index => index.GetDatabaseName() == "UX_Topics_Name" && index.IsUnique);
        Assert.Contains(topic.GetIndexes(), index => index.GetDatabaseName() == "IX_Topics_IsActive");
        Assert.Equal("Latin1_General_100_CI_AS", topic.FindProperty(nameof(Topic.Name))?.GetCollation());
    }

    [Fact]
    public void Question_mapping_uses_restrict_delete_and_topic_active_index()
    {
        var question = _model.FindEntityType(typeof(Question));
        var topicForeignKey = Assert.Single(question!.GetForeignKeys());

        Assert.Equal(DeleteBehavior.Restrict, topicForeignKey.DeleteBehavior);
        Assert.Contains(question.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_Questions_TopicId_IsActive" && index.Properties.Count == 2);
    }

    [Fact]
    public void Option_mapping_restricts_position_and_makes_it_unique_per_question()
    {
        var option = _model.FindEntityType(typeof(QuestionOption));

        Assert.NotNull(option);
        Assert.Contains(option.GetCheckConstraints(), constraint => constraint.Name == "CK_QuestionOptions_Position");
        Assert.Contains(option.GetIndexes(), index =>
            index.GetDatabaseName() == "UX_QuestionOptions_QuestionId_Position" && index.IsUnique);
        Assert.Equal("tinyint", option.FindProperty(nameof(QuestionOption.Position))?.GetColumnType());
        Assert.Equal(DeleteBehavior.Restrict, Assert.Single(option.GetForeignKeys()).DeleteBehavior);
    }
}
