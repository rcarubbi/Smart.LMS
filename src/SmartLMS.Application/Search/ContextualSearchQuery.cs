using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Authorization;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Search;

public sealed record SearchResultItem(Guid Id, string Description, string ResultType, int Percentual, string? Link = null);

/// <summary>
/// Ported from the legacy ContextualSearchService: matches active catalog entities by name
/// and enriches class/file results with the caller's watch progress.
/// </summary>
public sealed record ContextualSearchQuery(string Term, int? Page = null, int? PageSize = null)
    : IQuery<Paged<SearchResultItem>>;

internal sealed class ContextualSearchValidator : AbstractValidator<ContextualSearchQuery>
{
    public ContextualSearchValidator()
    {
        RuleFor(q => q.Term).NotEmpty().MaximumLength(100);
    }
}

internal sealed class ContextualSearchHandler(
    ISmartLmsDbContext context,
    ICurrentUser currentUser,
    ClassAccessPolicy accessPolicy)
    : IQueryHandler<ContextualSearchQuery, Paged<SearchResultItem>>
{
    public async ValueTask<ErrorOr<Paged<SearchResultItem>>> Handle(
        ContextualSearchQuery query,
        CancellationToken cancellationToken)
    {
        var term = query.Term.Trim();
        var userId = currentUser.UserId;

        var areas = await SpecificationEvaluator.Default
            .GetQuery(context.KnowledgeAreas.AsNoTracking(), new KnowledgeAreasSearchSpec(term))
            .Select(k => new SearchResultItem(k.Id, k.Name, "KnowledgeArea", 0, $"/catalog?area={k.Id}"))
            .ToListAsync(cancellationToken);

        var subjects = await SpecificationEvaluator.Default
            .GetQuery(context.Subjects.AsNoTracking(), new SubjectsSearchSpec(term))
            .Select(s => new SearchResultItem(s.Id, s.Name, "Subject", 0, $"/catalog?subject={s.Id}"))
            .ToListAsync(cancellationToken);

        var courses = await SpecificationEvaluator.Default
            .GetQuery(context.Courses.AsNoTracking(), new CoursesSearchSpec(term))
            .Select(c => new SearchResultItem(c.Id, c.Name, "Course", 0, $"/course/{c.Id}"))
            .ToListAsync(cancellationToken);

        var classes = await SpecificationEvaluator.Default
            .GetQuery(accessPolicy.AccessibleClasses(), new ClassesSearchSpec(term))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        List<SearchResultItem> classItems = [];
        if (classes.Count > 0 && userId is not null)
        {
            var classIds = classes.Select(c => c.Id).ToList();
            var progressSpec = new ClassAccessesByUserAndClassesSpec(userId.Value, classIds);
            var progress = await SpecificationEvaluator.Default
                .GetQuery(context.ClassAccesses.AsNoTracking(), progressSpec)
                .GroupBy(a => a.ClassId)
                .Select(g => new { ClassId = g.Key, Percentual = g.Max(a => a.Percentual) })
                .ToDictionaryAsync(p => p.ClassId, p => p.Percentual, cancellationToken);

            classItems = [.. classes.Select(c => new SearchResultItem(
                c.Id,
                c.Name,
                "Class",
                progress.TryGetValue(c.Id, out var percentual) ? percentual : 0,
                $"/class/{c.Id}"))];
        }
        else
        {
            classItems = [.. classes.Select(c => new SearchResultItem(c.Id, c.Name, "Class", 0, $"/class/{c.Id}"))];
        }

        var files = await SpecificationEvaluator.Default
            .GetQuery(context.Files.AsNoTracking().Where(f => accessPolicy.AccessibleClasses().Any(c => c.Id == f.ClassId)), new SupportFilesSearchSpec(term))
            .Select(f => new { f.Id, f.Name, f.StoragePath })
            .ToListAsync(cancellationToken);

        var fileItems = files
            .Select(f => new SearchResultItem(
                f.Id,
                f.Name,
                "File",
                0,
                $"/api/files/{f.Id}"))
            .ToList();

        if (fileItems.Count > 0 && userId is not null)
        {
            var downloadedSpec = new FileAccessesByUserAndFilesSpec(userId.Value, fileItems.Select(f => f.Id).ToList());
            var downloadedIds = await SpecificationEvaluator.Default
                .GetQuery(context.FileAccesses.AsNoTracking(), downloadedSpec)
                .Select(a => a.FileId)
                .ToListAsync(cancellationToken);

            fileItems = [.. fileItems.Select(f =>
                downloadedIds.Contains(f.Id) ? f with { Percentual = 100 } : f)];
        }

        var results = areas
            .Concat(subjects)
            .Concat(courses)
            .Concat(classItems)
            .Concat(fileItems)
            .ToList();

        return results.PageInMemory(query.Page, query.PageSize);
    }
}
