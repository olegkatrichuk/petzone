using PetZone.Contracts.Volunteers;

// Исправленный адрес (namespace)
namespace PetZone.Application.Volunteers.Commands;

// Команда просто оборачивает Request. Это нужно, чтобы отделить 
// транспортный слой (HTTP Request) от логики приложения.
public record CreateVolunteerCommand(CreateVolunteerRequest Request);