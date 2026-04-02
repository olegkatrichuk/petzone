using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetZone.VolunteerRequests.Application;
using PetZone.VolunteerRequests.Application.Commands.AddMessage;
using PetZone.VolunteerRequests.Application.Commands.ApproveVolunteerRequest;
using PetZone.VolunteerRequests.Application.Commands.CloseDiscussion;
using PetZone.VolunteerRequests.Application.Commands.CreateVolunteerRequest;
using PetZone.VolunteerRequests.Application.Commands.DeleteMessage;
using PetZone.VolunteerRequests.Application.Commands.EditMessage;
using PetZone.VolunteerRequests.Application.Commands.RejectVolunteerRequest;
using PetZone.VolunteerRequests.Application.Commands.SendForRevision;
using PetZone.VolunteerRequests.Application.Commands.TakeOnReview;
using PetZone.VolunteerRequests.Application.Commands.UpdateVolunteerRequest;
using PetZone.VolunteerRequests.Application.Queries.GetDiscussion;
using PetZone.VolunteerRequests.Application.Queries.GetDiscussionByRelationId;
using PetZone.VolunteerRequests.Application.Queries.GetRequestById;
using PetZone.VolunteerRequests.Application.Queries.GetRequestsByAdmin;
using PetZone.VolunteerRequests.Application.Queries.GetRequestsByUser;
using PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Infrastructure.Repositories;

namespace PetZone.VolunteerRequests.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVolunteerRequestsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VolunteerRequestsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        // UnitOfWork & Repositories
        services.AddScoped<IVolunteerRequestsUnitOfWork, VolunteerRequestsUnitOfWork>();
        services.AddScoped<IVolunteerRequestRepository, VolunteerRequestRepository>();
        services.AddScoped<IRejectedUserRepository, RejectedUserRepository>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(CreateVolunteerRequestHandler).Assembly));

        // Handlers
        services.AddScoped<CreateVolunteerRequestHandler>();
        services.AddScoped<TakeOnReviewHandler>();
        services.AddScoped<SendForRevisionHandler>();
        services.AddScoped<RejectVolunteerRequestHandler>();
        services.AddScoped<ApproveVolunteerRequestHandler>();
        services.AddScoped<UpdateVolunteerRequestHandler>();
        services.AddScoped<GetUnreviewedRequestsHandler>();
        services.AddScoped<GetRequestsByAdminHandler>();
        services.AddScoped<GetRequestsByUserHandler>();
        services.AddScoped<GetRequestByIdHandler>();
        // Repositories
        services.AddScoped<IDiscussionRepository, DiscussionRepository>();


        services.AddScoped<AddMessageHandler>();
        services.AddScoped<DeleteMessageHandler>();
        services.AddScoped<EditMessageHandler>();
        services.AddScoped<CloseDiscussionHandler>();
        services.AddScoped<GetDiscussionHandler>();
        services.AddScoped<GetDiscussionByRelationIdHandler>();

        return services;
    }
}