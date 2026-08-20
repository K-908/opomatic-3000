namespace OpoMatic3000.Domain.Questions;

public sealed record QuestionOptionDefinition(
    string Text,
    byte Position,
    bool IsCorrect);
