using System.ComponentModel.DataAnnotations;

namespace MyQuery.Models
{
    public class LoginModel
    {
        public int StaffID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Position { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public string? Role { get; set; }
        public DateTime? Last_Login { get; set; }
        public string? ProfilePhoto { get; set; }
    }
}
