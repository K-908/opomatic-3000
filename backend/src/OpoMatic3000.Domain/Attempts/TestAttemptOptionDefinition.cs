using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Domain.Attempts;

public sealed record TestAttemptOptionDefinition(
    QuestionOption OriginalOption,
    byte DisplayOrder,
    bool IsSelected);
