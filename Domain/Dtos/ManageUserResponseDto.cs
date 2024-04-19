using Domain.Entities;

namespace Domain.Dtos;
public record ManageUserResponseDto(
    bool IsSucceeded,
    ApplicationUser? User,
    string? VerificationCode,
    IEnumerable<string>? Errors
);