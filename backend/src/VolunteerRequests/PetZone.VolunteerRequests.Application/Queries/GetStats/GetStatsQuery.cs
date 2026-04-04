namespace PetZone.VolunteerRequests.Application.Queries.GetStats;

public record GetStatsQuery;

public record VolunteerRequestStats(
    int Total,
    int Submitted,
    int OnReview,
    int RevisionRequired,
    int Approved,
    int Rejected
);
