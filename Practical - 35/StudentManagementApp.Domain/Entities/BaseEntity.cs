namespace StudentManagementApp.Domain.Entities;

/// <summary>
/// Base class for all domain entities. Provides auto-managed audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    /// <summary>Set once at creation; never modified by the repository thereafter.</summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>Refreshed on every write to the repository.</summary>
    public DateTime UpdatedOn { get; set; }
}
