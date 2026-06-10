using System.ComponentModel.DataAnnotations;

namespace Libraryapp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Per-user random salt stored as Base64. Combined with password before hashing.
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Salt { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
