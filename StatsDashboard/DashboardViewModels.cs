namespace StatsDashboard;

public abstract record DashboardViewModel
{
    public abstract string Title { get; }
    public virtual Period? Period => null;
    public virtual string? LeagueId => null;
    public virtual string? LeagueName => null;
    public virtual string Action => "Overview";
}

public sealed record OverviewViewModel(OverviewData Data, IReadOnlyDictionary<string, string> Names,
    IReadOnlyDictionary<string, RunTiming> Timings) : DashboardViewModel
{
    public override string Title => "The league overview";
    public override Period? Period => Data.Period;
}

public sealed record LeagueViewModel(string Id, string Name, LeagueData Data, RunsData Runs, MemoriesData Memories,
    string? ScheduleError = null) : DashboardViewModel
{
    public override string Title => string.IsNullOrWhiteSpace(Name) ? Id : Name;
    public override Period? Period => Data.Period;
    public override string? LeagueId => Id;
    public override string? LeagueName => Name;
    public override string Action => "League";
}

public sealed record HistoryViewModel(string Id, string Name, HistoryData Data) : DashboardViewModel
{
    public override string Title => "Call history · " + Name;
    public override Period? Period => Data.Period;
    public override string? LeagueId => Id;
    public override string? LeagueName => Name;
    public override string Action => "History";
}

public sealed record RunsViewModel(string Id, string Name, RunsData Data) : DashboardViewModel
{
    public override string Title => "Runs · " + Name;
    public override Period? Period => Data.Period;
    public override string? LeagueId => Id;
    public override string? LeagueName => Name;
    public override string Action => "Runs";
}

public sealed record ErrorViewModel : DashboardViewModel
{
    public override string Title => "Dashboard unavailable";
}
