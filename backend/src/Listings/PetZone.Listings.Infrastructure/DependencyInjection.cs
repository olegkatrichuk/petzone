using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetZone.Listings.Application;
using PetZone.Listings.Application.Commands.CreateListing;
using PetZone.Listings.Application.Commands.DeleteListing;
using PetZone.Listings.Application.Commands.MarkAdopted;
using PetZone.Listings.Application.Commands.UpdateListing;
using PetZone.Listings.Domain;
using PetZone.Listings.Infrastructure.Queries;
using PetZone.Listings.Infrastructure.Repositories;
using PetZone.Listings.Infrastructure.Services;

namespace PetZone.Listings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddListingsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ListingsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IListingsUnitOfWork, ListingsUnitOfWork>();

        services.AddScoped<CreateListingService>();
        services.AddScoped<UpdateListingService>();
        services.AddScoped<DeleteListingService>();
        services.AddScoped<MarkAdoptedService>();

        services.AddScoped<AddListingPhotoService>();
        services.AddScoped<RemoveListingPhotoService>();

        services.AddScoped<GetAllListingsHandler>();
        services.AddScoped<GetListingByIdHandler>();
        services.AddScoped<GetMyListingsHandler>();

        return services;
    }
}
