using CSharpFunctionalExtensions;
using PetZone.Accounts.Contracts.Dtos;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.GetUserInfo;

public record GetUserInfoQuery(Guid UserId);