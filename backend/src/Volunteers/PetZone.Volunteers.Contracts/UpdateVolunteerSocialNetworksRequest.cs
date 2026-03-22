namespace PetZone.Volunteers.Contracts;

public record UpdateVolunteerSocialNetworksRequest(
    IEnumerable<SocialNetworkDto> SocialNetworks);