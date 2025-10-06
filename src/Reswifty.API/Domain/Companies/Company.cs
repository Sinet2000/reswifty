using Reswifty.API.Domain.Common;

namespace Reswifty.API.Domain.Companies;

public class Company : BaseEntity
{
    public string Name { get; private set; } = default!;
    public CompanyType Type { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Company()
    {
    } // EF

    public Company(string name, CompanyType type, string? description)
    {
        SetName(name);
        Type = type;
        Description = description?.Trim();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));
        Name = name.Trim();
        Touch();
    }

    public void SetType(CompanyType type)
    {
        Type = type;
        Touch();
    }

    public void SetDescription(string? desc)
    {
        Description = desc?.Trim();
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}