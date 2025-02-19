using System.ComponentModel.DataAnnotations;

public class EmailDogrulamaViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string VerificationCode { get; set; }
}
