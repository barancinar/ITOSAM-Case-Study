using System.ComponentModel.DataAnnotations;

namespace ItoCase.Core.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}