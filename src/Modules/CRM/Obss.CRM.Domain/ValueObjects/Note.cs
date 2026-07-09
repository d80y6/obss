namespace Obss.CRM.Domain.ValueObjects;

public sealed record Note(
    DateTime Date,
    string Author,
    string Text);
