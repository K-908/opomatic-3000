using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Domain.Attempts;

public sealed class TestAttemptOption
{
    private TestAttemptOption()
    {
    }

    internal TestAttemptOption(
        TestAttemptQuestion testAttemptQuestion,
        TestAttemptOptionDefinition definition)
    {
        TestAttemptQuestion = testAttemptQuestion;
        OriginalOption = definition.OriginalOption;
        TextSnapshot = definition.OriginalOption.Text;
        DisplayOrder = definition.DisplayOrder;
        IsCorrect = definition.OriginalOption.IsCorrect;
        IsSelected = definition.IsSelected;
    }

    public long Id { get; private set; }

    public long TestAttemptQuestionId { get; private set; }

    public TestAttemptQuestion TestAttemptQuestion { get; private set; } = null!;

    public int OriginalOptionId { get; private set; }

    public QuestionOption OriginalOption { get; private set; } = null!;

    public string TextSnapshot { get; private set; } = string.Empty;

    public byte DisplayOrder { get; private set; }

    public bool IsCorrect { get; private set; }

    public bool IsSelected { get; private set; }
}
