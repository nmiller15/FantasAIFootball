using FantasAIFootball.Models.Interactions;
using FantasAIFootball.Services;

namespace FantasAIFootball.Extensions;

public static class AgentExtensions
{
    public static void RegisterTools(this Agent agent)
    {
        agent.AddTool(new Function
        {
            Name = "GetLeague",
            Description = "Gets the details and configuration of the fantasy football league. Returns a League."
        });

        agent.AddTool(new Function
        {
            Name = "GetScoringSettings",
            Description = "Gets the scoring settings that determine how fantasy points are awarded in the league. Returns a ScoringSettings."
        });

        agent.AddTool(new Function
        {
            Name = "GetRosters",
            Description = "Gets the rosters for all teams in the league. Returns an array of Roster."
        });

        agent.AddTool(new Function
        {
            Name = "GetMyRoster",
            Description = "Gets the roster for the current user's team. Returns a Roster."
        });

        agent.AddTool(new Function
        {
            Name = "GetUserRoster",
            Description = "Gets the roster for a specific user's team by their user id. Returns a Roster.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["userId"] = new FunctionParameter { Type = "string", Description = "The id of the user whose roster to retrieve." }
                },
                Required = ["userId"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "GetUser",
            Description = "Gets a user in the league by their username. Returns a User.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["username"] = new FunctionParameter { Type = "string", Description = "The username of the user to retrieve." }
                },
                Required = ["username"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "GetPlayer",
            Description = "Gets the details of an NFL player by their player id. Returns a Player.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["playerId"] = new FunctionParameter { Type = "string", Description = "The id of the player to retrieve." }
                },
                Required = ["playerId"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "GetNflState",
            Description = "Gets the current state of the NFL season, including the current week. Returns an NflState."
        });

        agent.AddTool(new Function
        {
            Name = "GetMatchups",
            Description = "Gets all matchups for a given week. Returns an array of Matchup.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["week"] = new FunctionParameter { Type = "integer", Description = "The week number to retrieve matchups for." }
                },
                Required = ["week"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "GetMatchupByRosterId",
            Description = "Gets the matchup for a specific roster in a given week. Returns a Matchup.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["rosterId"] = new FunctionParameter { Type = "integer", Description = "The id of the roster whose matchup to retrieve." },
                    ["week"] = new FunctionParameter { Type = "integer", Description = "The week number to retrieve the matchup for." }
                },
                Required = ["rosterId", "week"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "GetCoorespondingMatchup",
            Description = "Gets the opposing matchup that corresponds to a given matchup id in a given week. Returns a Matchup.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["matchupId"] = new FunctionParameter { Type = "integer", Description = "The id of the matchup to find the corresponding matchup for." },
                    ["week"] = new FunctionParameter { Type = "integer", Description = "The week number the matchup belongs to." }
                },
                Required = ["matchupId", "week"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "WebSearch",
            Description = "Searches the web for information, optionally constrained to a date range. Returns a QueryResult.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["query"] = new FunctionParameter { Type = "string", Description = "The search query." },
                    ["startDate"] = new FunctionParameter { Type = "string", Description = "Optional. The earliest date (ISO 8601) to include results from." },
                    ["endDate"] = new FunctionParameter { Type = "string", Description = "Optional. The latest date (ISO 8601) to include results from." }
                },
                Required = ["query"]
            }
        });

        agent.AddTool(new Function
        {
            Name = "ScrapeUrls",
            Description = "Scrapes the content of a single URL. Returns an array of ExtractResult.",
            Parameters = new FunctionParameters
            {
                Properties = new()
                {
                    ["url"] = new FunctionParameter { Type = "string", Description = "The URL to scrape." }
                },
                Required = ["url"]
            }
        });
    }
}
