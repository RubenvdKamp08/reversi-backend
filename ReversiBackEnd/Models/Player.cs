using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ReversiBackEnd.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiBackEnd.Models
{
    public class Player : IValidatableObject
    {
        public int UserId { get; set; }

        //[Required(ErrorMessage = "Gebruikersnaam is verplicht")]
        [DisplayName("Gebruikersnaam")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Email is verplicht")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [DisplayName("Wachtwoord")]
        public string Password { get; set; }

        public byte[] Salt { get; set; }

        public string Role { get; set; }

        public string Token { get; set; }

        [NotMapped]
        [DisplayName("Gebruikersnaam of Email")]
        public string InputName { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.InputName == null)
            {
                if (Password.Length < 8)
                {
                    yield return new ValidationResult($"wachtwoord moet meer dan acht tekens hebben");
                }
                if (!Password.Any(C => char.IsUpper(C) || char.IsLower(C) || char.IsDigit(C)))
                {
                    yield return new ValidationResult($"Wachtwoord moet minimaal een kleine en grote letter bevatten en een cijfer hebben");
                }
                if (UserManager.CheckEmail(this))
                {
                    yield return new ValidationResult(@"Email is al in gebruik");
                }
                if (UserManager.CheckUserName(this))
                {
                    yield return new ValidationResult(@"Gebruikersnaam is al in gebruik");
                }
            }
        }
    }
}
