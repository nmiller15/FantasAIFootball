# Tool Call Stats Dashboard

A standalone ASP.NET Core site for the `ToolCallRecords` table written by the main FantasAIFootball app. It opens `stats.db` **read-only** and displays totals by league, tool, and day. On a league page, expand a tool to see the time and parameters of its calls, or expand a day to see a count by tool for that day. A separate linked `/league/{leagueId}/history` page offers chronological, paginated call history (20 per page).

League names come from Sleeper's public `GET /v1/league/{leagueId}` API. The overview shows the name with the ID underneath; league pages use the name as their heading. Names are cached for six hours. If Sleeper is unavailable or a league cannot be found, the ID is shown instead (and the failed lookup is cached for five minutes). The dashboard still works without Sleeper access.

## Run

Requires .NET 10. Run from the repository root:

```sh
ASPNETCORE_URLS=http://127.0.0.1:5080 dotnet run --project StatsDashboard/StatsDashboard.csproj
```

By default the dashboard uses the writer's `UserData.GetAppDataLocation()` helper and reads `stats.db` in that directory. No database configuration is needed if both apps run under the same OS user on the same machine. If the dashboard runs under a different user or on another machine, set `STATS_DB_PATH` to an absolute path to the writer's database (or to a readable copy, including its SQLite journal/WAL files if applicable). A missing database displays an empty state; a database without the expected table displays an error and logs the details server-side. The dashboard never creates or changes the database.

For deployment, publish independently:

```sh
dotnet publish StatsDashboard/StatsDashboard.csproj -c Release -o ./dashboard-publish
ASPNETCORE_URLS=http://127.0.0.1:5080 dotnet ./dashboard-publish/StatsDashboard.dll
```

The GitHub workflow also publishes a self-contained Linux build on pushes to `main`, published releases, and manual runs. It deploys the dashboard to `/opt/FantasAIFootball/dashboard/` (preserved when the main app is synced with `--delete`), installs `StatsDashboard/deploy/fantasaifootball-dashboard.service`, restarts the dashboard, then restarts Caddy. The EC2 `nolan` user must be able to write to `/opt/FantasAIFootball/` and run `sudo -n` for systemd service installation and restarts. The service runs as `nolan` and therefore uses that user's default app-data path for `stats.db`; if the writer uses another account, supply `STATS_DB_PATH` through a systemd override.

Configure your subdomain in Caddy to reverse proxy to `127.0.0.1:5080`; the workflow restarts Caddy but cannot configure an unspecified hostname. **Require authentication or restrict access at the reverse proxy:** stored tool arguments may contain private data, and the dashboard itself has no login. Keep the writer's database off the public web root. Visit `/` for the league overview and select a league for tool breakdown and daily activity. The default range is 30 days; 7 days and all time are also available. Dates are grouped by the local date saved by the writer, not converted to the browser's time zone. Counts represent attempted calls (including unsuccessful ones).
