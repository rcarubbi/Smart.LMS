using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;

namespace SmartLMS.Application.Search;

public sealed record SearchResultItem(Guid Id, string Description, string ResultType, int Percentual);

/// <summary>
/// Ported from the legacy ContextualSearchService: matches active catalog entities by name
/// and enriches class/file results with the caller's watch progress.
/// </summary>
public sealed record ContextualSearchQuery(string Term) : IQuery<IReadOnlyList<SearchResultItem>>;

internal sealed class ContextualSearchValidator : AbstractValidator<ContextualSearchQuery>
{
    public ContextualSearchValidator()
    {
        RuleFor(q => q.Term).NotEmpty().MaximumLength(100);
    }
}

internal sealed class ContextualSearchHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<ContextualSearchQuery, IReadOnlyList<SearchResultItem>>
{
    public async ValueTask<Result<IReadOnlyList<SearchResultItem>>> Handle(
        ContextualSearchQuery query,
        CancellationToken cancellationToken)
    {
        var term = query.Term.Trim();
        var userId = currentUser.UserId;

        var areas = await context.KnowledgeAreas
            .Where(k => k.Active && k.Name.Contains(term))
            .Select(k => new SearchResultItem(k.Id, k.Name, "KnowledgeArea", 0))
            .ToListAsync(cancellationToken);

        var subjects = await context.Subjects
            .Where(s => s.Active && s.Name.Contains(term))
            .Select(s => new SearchResultItem(s.Id, s.Name, "Subject", 0))
            .ToListAsync(cancellationToken);

        var courses = await context.Courses
            .Where(c => c.Active && c.Name.Contains(term))
            .Select(c => new SearchResultItem(c.Id, c.Name, "Course", 0))
            .ToListAsync(cancellationToken);

        var classes = await context.Classes
            .Where(c => c.Active && c.Name.Contains(term))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        List<SearchResultItem> classItems = [];
        if (classes.Count > 0 && userId is not null)
        {
            var progress = await context.ClassAccesses
                .Where(a => a.UserId == userId && classes.Select(c => c.Id).Contains(a.ClassId))
                .GroupBy(a => a.ClassId)
                .Select(g => new { ClassId = g.Key, Percentual = g.Max(a => a.Percentual) })
                .ToDictionaryAsync(p => p.ClassId, p => p.Percentual, cancellationToken);

            classItems = [.. classes.Select(c => new SearchResultItem(
                c.Id,
                c.Name,
                "Class",
                progress.TryGetValue(c.Id, out var percentual) ? percentual : 0))];
        }
        else
        {
            classItems = [.. classes.Select(c => new SearchResultItem(c.Id, c.Name, "Class", 0))];
        }

        var files = await context.Files
            .Where(f => f.Active && f.Name.Contains(term))
            .Select(f => new SearchResultItem(f.Id, f.Name, "File", 0))
            .ToListAsync(cancellationToken);

        List<SearchResultItem> fileItems = files;
        if (files.Count > 0 && userId is not null)
        {
            var downloadedIds = await context.FileAccesses
                .Where(a => a.UserId == userId && files.Select(f => f.Id).Contains(a.FileId))
                .Select(a => a.FileId)
                .ToListAsync(cancellationToken);

            fileItems = [.. files.Select(f =>
                downloadedIds.Contains(f.Id) ? f with { Percentual = 100 } : f)];
        }

        var results = areas
            .Concat(subjects)
            .Concat(courses)
            .Concat(classItems)
            .Concat(fileItems)
            .ToList();

        return Result.Success<IReadOnlyList<SearchResultItem>>(results);
    }
}
