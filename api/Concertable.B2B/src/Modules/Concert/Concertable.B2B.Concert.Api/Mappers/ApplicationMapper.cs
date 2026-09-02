using Concertable.B2B.Concert.Api.Responses;
using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Concert.Application.Workflow.Capabilities;
using Concertable.B2B.Concert.Domain.Lifecycle;
using Concertable.Shared.Api.Http;

namespace Concertable.B2B.Concert.Api.Mappers;

internal sealed class ApplicationMapper : IApplicationMapper
{
    private readonly IConcertWorkflowCapabilityRegistry registry;

    public ApplicationMapper(IConcertWorkflowCapabilityRegistry registry)
        => this.registry = registry;

    public ApplicationResponse<VenueApplicationActions> ToVenueResponse(ApplicationDto dto)
    {
        var isPending = dto.State == LifecycleState.Applied;
        var isCancellable = dto.State is LifecycleState.Accepted or LifecycleState.PaymentFailed;

        return ToResponse(
            dto,
            dto.Status,
            new VenueApplicationActions(
                Accept: isPending
                    ? ActionLink.Post($"/api/application/{dto.Id}/accept")
                    : null,
                Checkout: isPending && registry.Has<IAcceptsCheckout>(dto.Opportunity.Deal.DealType)
                    ? ActionLink.Post($"/api/application/{dto.Id}/checkout")
                    : null,
                Decline: isPending ? ActionLink.Post($"/api/application/{dto.Id}/reject") : null,
                Cancel: isCancellable ? ActionLink.Post($"/api/application/{dto.Id}/cancel") : null,
                Contract: ContractAction(dto)));
    }

    public IEnumerable<ApplicationResponse<VenueApplicationActions>> ToVenueResponses(IEnumerable<ApplicationDto> dtos) =>
        dtos.Select(ToVenueResponse);

    public ApplicationResponse<ArtistApplicationActions> ToArtistResponse(ApplicationDto dto)
    {
        var checkoutCapable = registry.Has<IAcceptsCheckout>(dto.Opportunity.Deal.DealType);
        var awaitingPayment = checkoutCapable
            && dto.State is LifecycleState.Accepted or LifecycleState.PaymentFailed;
        var status = dto.State switch
        {
            LifecycleState.Accepted or LifecycleState.PaymentFailed when awaitingPayment => ApplicationStatus.AwaitingPayment,
            LifecycleState.Booked or LifecycleState.AwaitingSettlement or LifecycleState.SettlementFailed or LifecycleState.Complete => ApplicationStatus.Confirmed,
            _ => dto.Status
        };

        var canWithdraw = dto.State is LifecycleState.Applied or LifecycleState.Accepted or LifecycleState.PaymentFailed;
        return ToResponse(
            dto,
            status,
            new ArtistApplicationActions(
                Withdraw: canWithdraw
                    ? ActionLink.Post($"/api/application/{dto.Id}/withdraw")
                    : null,
                Contract: ContractAction(dto)));
    }

    public IEnumerable<ApplicationResponse<ArtistApplicationActions>> ToArtistResponses(IEnumerable<ApplicationDto> dtos) =>
        dtos.Select(ToArtistResponse);

    private static ApplicationResponse<TActions> ToResponse<TActions>(
        ApplicationDto dto,
        ApplicationStatus status,
        TActions actions) =>
        new(
            dto.Id,
            dto.Artist,
            new OpportunitySummaryResponse(
                dto.Opportunity.Id,
                dto.Opportunity.VenueId,
                dto.Opportunity.VenueName,
                dto.Opportunity.StartDate,
                dto.Opportunity.EndDate,
                dto.Opportunity.Genres,
                dto.Opportunity.Deal),
            status,
            actions);

    private static ActionLink? ContractAction(ApplicationDto dto) =>
        dto.ContractId is not null
            ? ActionLink.Get($"/api/application/{dto.Id}/contract/pdf")
            : null;
}
