namespace PetZone.Contracts.Volunteers;

public record UpdateVolunteerSocialNetworksRequest(
    IEnumerable<SocialNetworkDto> SocialNetworks);