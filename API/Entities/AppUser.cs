using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    public DateTime DateOfBirth { get; set; }
    public string KnownAs { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastActive { get; set; } = DateTime.Now;
    public string Gender { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public string LookingFor { get; set; } = string.Empty;
    public string Interests { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public String Country { get; set; } = String.Empty;
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    public ICollection<UserLike> LikedByUsers { get; set; } = new List<UserLike>();
    public ICollection<UserLike> LikedUsers { get; set; } = new List<UserLike>();
    public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
    public ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
