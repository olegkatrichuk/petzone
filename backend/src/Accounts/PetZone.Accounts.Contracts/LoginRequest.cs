using System.ComponentModel.DataAnnotations;

namespace PetZone.Accounts.Contracts;

public record LoginRequest(
    [Required][EmailAddress][MaxLength(256)] string Email,
    [Required][MaxLength(128)] string Password);