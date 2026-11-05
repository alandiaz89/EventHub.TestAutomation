namespace EventHub.Core.Cleanup;

/// <summary>Collects cleanup actions during a test and runs them (newest first) in teardown.</summary>
public sealed class CleanupRegistry
{
    private readonly List<Func<Task>> _actions = [];

    public void Add(Func<Task> cleanup) => _actions.Add(cleanup);

    /// <summary>Runs every action; a failing cleanup never stops the others. Returns the failures.</summary>
    public async Task<IReadOnlyList<Exception>> RunAsync()
    {
        var failures = new List<Exception>();
        for (var i = _actions.Count - 1; i >= 0; i--)
        {
            try
            {
                await _actions[i]();
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        }

        _actions.Clear();
        return failures;
    }
}
