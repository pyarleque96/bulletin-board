using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Reports;

public record DismissReportCommand(Guid ReportId) : IRequest;

public sealed class DismissReportCommandHandler(
    IRepository<Report> reportRepository,
    ICurrentUserService currentUserService,
    ILogger<DismissReportCommandHandler> logger)
    : IRequestHandler<DismissReportCommand>
{
    public async Task Handle(DismissReportCommand request, CancellationToken ct)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId, ct)
            ?? throw new InvalidOperationException($"Report '{request.ReportId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        report.Dismiss(adminId);
        reportRepository.Update(report);
        await reportRepository.SaveChangesAsync(ct);

        logger.LogInformation("Report {ReportId} dismissed by {AdminId}", request.ReportId, adminId);
    }
}

public sealed class DismissReportCommandValidator : AbstractValidator<DismissReportCommand>
{
    public DismissReportCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
