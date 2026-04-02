using System.ComponentModel.DataAnnotations;

namespace PetZone.Accounts.Contracts;

public record RegisterRequest(
    [Required][MaxLength(100)] string FirstName,
    [Required][MaxLength(100)] string LastName,
    [Required][EmailAddress][MaxLength(256)] string Email,
    [Required][MinLength(8)][MaxLength(128)] string Password);
