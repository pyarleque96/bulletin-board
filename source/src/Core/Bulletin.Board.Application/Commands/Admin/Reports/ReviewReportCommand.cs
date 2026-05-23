using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Reports;

public record ReviewReportCommand(Guid ReportId) : IRequest;

public sealed class ReviewReportCommandHandler(
    IRepository<Report> reportRepository,
    ICurrentUserService currentUserService,
    ILogger<ReviewReportCommandHandler> logger)
    : IRequestHandler<ReviewReportCommand>
{
    public async Task Handle(ReviewReportCommand request, CancellationToken ct)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId, ct)
            ?? throw new InvalidOperationException($"Report '{request.ReportId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        report.Review(adminId);
        reportRepository.Update(report);
        await reportRepository.SaveChangesAsync(ct);

        logger.LogInformation("Report {ReportId} reviewed by {AdminId}", request.ReportId, adminId);
    }
}

public sealed class ReviewReportCommandValidator : AbstractValidator<ReviewReportCommand>
{
    public ReviewReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
