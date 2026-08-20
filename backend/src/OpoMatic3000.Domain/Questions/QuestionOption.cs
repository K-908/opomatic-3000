namespace OpoMatic3000.Domain.Questions;

public sealed class QuestionOption
{
    private QuestionOption()
    {
    }

    internal QuestionOption(Question question, string text, byte position, bool isCorrect)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (text.Length > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(text), "Option text cannot exceed 1000 characters.");
        }

        Question = question;
        Text = text.Trim();
        Position = position;
        IsCorrect = isCorrect;
    }

    public int Id { get; private set; }

    public int QuestionId { get; private set; }

    public Question Question { get; private set; } = null!;

    public string Text { get; private set; } = string.Empty;

    public byte Position { get; private set; }

    public bool IsCorrect { get; private set; }

    internal void Update(string text, bool isCorrect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var normalizedText = text.Trim();
        if (normalizedText.Length > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(text), "Option text cannot exceed 1000 characters.");
        }

        Text = normalizedText;
        IsCorrect = isCorrect;
    }
}
