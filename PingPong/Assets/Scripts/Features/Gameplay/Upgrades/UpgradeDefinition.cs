using System;

public sealed class UpgradeDefinition
{
    public UpgradeId Id { get; }
    public string Name { get; }
    public string Description { get; }

    public UpgradeDefinition(UpgradeId id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название улучшения не может быть пустым.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание улучшения не может быть пустым.", nameof(description));

        Id = id;
        Name = name;
        Description = description;
    }
}
