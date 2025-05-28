using System.Collections.Concurrent;
using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;

public class PrintJobGroupingService : IPrintJobGroupingService
{
    private readonly ILoggingService _logger;
    private readonly ConcurrentDictionary<string, PrintJobGroup> _groups;
    private TimeSpan _groupTimeout;

    public PrintJobGroupingService(ILoggingService logger)
    {
        _logger = logger;
        _groups = new ConcurrentDictionary<string, PrintJobGroup>();
        _groupTimeout = TimeSpan.FromMinutes(60); // Default timeout
    }

    public event EventHandler<PrintJobGroup> GroupCreated;
    public event EventHandler<PrintJobGroup> GroupModified;
    public event EventHandler<PrintJobGroup> GroupCompleted;

    public TimeSpan GroupTimeout => _groupTimeout;
    public IReadOnlyList<PrintJobGroup> ActiveGroups => _groups.Values.ToList();

    public async Task<PrintJobGroup> ProcessNewJobAsync(PrintJob job)
    {
        try
        {
            var groupKey = GetGroupKey(job);
            if (!_groups.TryGetValue(groupKey, out var group))
            {
                group = new PrintJobGroup
                {
                    GroupId = Guid.NewGuid().ToString(),
                    DocumentName = job.DocumentName,
                    Owner = job.Owner,
                    CreatedTime = DateTime.Now,
                    Jobs = new List<PrintJob>()
                };
                _groups[groupKey] = group;
                _logger.LogInformation($"Created new group {group.GroupId} for document {job.DocumentName}");
                GroupCreated?.Invoke(this, group);
            }

            group.AddJob(job);
            _logger.LogInformation($"Added job {job.JobId} to group {group.GroupId}");
            GroupModified?.Invoke(this, group);

            if (group.IsComplete)
            {
                GroupCompleted?.Invoke(this, group);
            }

            return group;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing new job {job.JobId}", ex);
            throw;
        }
    }

    public void UpdateGroupTimeout(TimeSpan timeout)
    {
        _groupTimeout = timeout;
        _logger.LogInformation($"Updated group timeout to {timeout.TotalMinutes} minutes");
    }

    public void CleanupOldGroups()
    {
        var cutoffTime = DateTime.Now - _groupTimeout;
        var oldGroups = _groups.Where(g => g.Value.LastModifiedTime < cutoffTime).ToList();

        foreach (var group in oldGroups)
        {
            if (_groups.TryRemove(group.Key, out var removedGroup))
            {
                _logger.LogInformation($"Removed old group {removedGroup.GroupId} for document {removedGroup.DocumentName}");
            }
        }
    }

    private string GetGroupKey(PrintJob job)
    {
        return $"{job.DocumentName}_{job.Owner}";
    }
} 
