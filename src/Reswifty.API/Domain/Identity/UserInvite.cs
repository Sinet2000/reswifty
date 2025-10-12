using System.ComponentModel.DataAnnotations;
using Dexlaris.Core.Models;

namespace Reswifty.API.Domain.Identity;

public class UserInvite : BaseEntity
{
    private UserInvite()
    {
    }

    public UserInvite(DateTime expire, int userId, Guid key)
    {
        UserId = userId;
        Expire = expire;
        Key = key;
        CreatedBy = userId.ToString();
    }

    public UserInvite(DateTime expire, Guid key)
    {
        Expire = expire;
        Key = key;
        CreatedBy = null!;
    }

    [Required]
    public int UserId { get; private set; }

    public User User { get; private set; } = null!;

    public DateTime Expire { get; private set; }

    public Guid Key { get; private set; }

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public int CreatedById { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public int? ModifiedById { get; set; }

    public void SetExpire(DateTime expireDate)
    {
        Expire = expireDate;
    }
}