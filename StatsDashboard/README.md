# Tool Call Stats Dashboard

A standalone ASP.NET Core site for the `ToolCallRecords` and `Runs` tables written by the main FantasAIFootball app. It reads `stats.db` **read-only** and reads `schedule.db` read-only except when scheduling a new run. It displays totals by league, tool, and day. On a league page, expand a recent run to see its status, report subject, and saved HTML email body; the tool breakdown and daily activity follow below. Separate linked `/league/{leagueId}/history` and `/league/{leagueId}/runs` pages offer paginated call history and scheduled runs (20 per page). Completed reports are retained in the schedule database; older completed runs whose reports were previously cleared cannot be recovered.

The dashboard uses MVC: `Controllers/DashboardController.cs` loads the data, `DashboardViewModels.cs` supplies the views, and `Views/Dashboard/` and `Views/Shared/` render the pages. The saved report HTML is displayed only in a sandboxed iframe.

League names come from Sleeper's public `GET /v1/league/{leagueId}` API. The overview shows the name with the ID underneath, plus the last execution, next future run, and any due or overdue run for each league; league pages show the five most recent runs and their expandable reports above tool activity. Names are cached for six hours. If Sleeper is unavailable or a league cannot be found, the ID is shown instead (and the failed lookup is cached for five minutes). The dashboard still works without Sleeper access.

## Run

Requires .NET 10. Run from the repository root:

```sh
ASPNETCORE_URLS=http://127.0.0.1:5080 dotnet run --project StatsDashboard/StatsDashboard.csproj
```

By default the dashboard uses the writer's `UserData.GetAppDataLocation()` helper and reads `stats.db` and `schedule.db` in that directory. No database configuration is needed if both apps run under the same OS user on the same machine. If the dashboard runs under a different user or on another machine, set `STATS_DB_PATH` and `SCHEDULE_DB_PATH` to absolute paths to the writer's databases (or to copies, including their SQLite journal/WAL files if applicable). To schedule runs, the dashboard user must have write access to the writer's **actual** `schedule.db` and its directory for SQLite journal/WAL files. The dashboard and writer should use the same local time zone. A missing database displays an empty state; a stats database without the expected tool-call table displays an error and logs the details server-side. The dashboard never creates or migrates either database; its only write is an inserted scheduled run.

On a league page, enter an optional note and select **Schedule run** to insert a run due at the top of the current hour in the dashboard server's local time. The writer will pick it up on its next scheduler tick (after any earlier due runs). Only leagues already present in the dashboard can be scheduled. The form uses an anti-forgery token and redirects after a successful submission. Expand a run with a saved report and select **Open full-screen report** to view the HTML in a separate full-window sandboxed viewer. The viewer overrides the email's 600px presentation-table width without changing the saved email or its inline preview.

Older `schedule.db` files may not yet have execution or report columns. The dashboard still displays their scheduled runs, but cannot show execution status or reports until the main app migrates the database on startup. A missing `Runs` table is shown as an empty runs view rather than breaking the dashboard.

For deployment, publish independently:

```sh
dotnet publish StatsDashboard/StatsDashboard.csproj -c Release -o ./dashboard-publish
ASPNETCORE_URLS=http://127.0.0.1:5080 dotnet ./dashboard-publish/StatsDashboard.dll
```

The GitHub workflow also publishes a self-contained Linux build on pushes to `main`, published releases, and manual runs. It deploys the dashboard to `/opt/FantasAIFootball/dashboard/` (preserved when the main app is synced with `--delete`), installs `StatsDashboard/deploy/fantasaifootball-dashboard.service`, restarts the dashboard, then restarts Caddy. The EC2 `nolan` user must be able to write to `/opt/FantasAIFootball/` and run `sudo -n` for systemd service installation and restarts. The service runs as `nolan` and therefore uses that user's default app-data path for `stats.db`; if the writer uses another account, supply `STATS_DB_PATH` through a systemd override.

Configure your subdomain in Caddy to reverse proxy to `127.0.0.1:5080`; the workflow restarts Caddy but cannot configure an unspecified hostname. **Require authentication or restrict access at the reverse proxy:** stored tool arguments and email reports may contain private data, and anyone who can access the dashboard can now schedule runs; the dashboard itself has no login. Keep the writer's databases off the public web root. Visit `/` for the league overview and select a league for tool breakdown, daily activity, and runs. The default range is 30 days; 7 days and all time are also available. Dates are grouped by the local date saved by the writer, not converted to the browser's time zone. Counts represent attempted calls (including unsuccessful ones).
