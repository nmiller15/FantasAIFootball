using FantasAIFootball.Models.Interactions;
using FantasAIFootball.Repositories;

namespace FantasAIFootball.Services;

public class Agent
{
    private readonly InteractionsRepository _interactionsRepository;
    private readonly FunctionService? _functionService;

    public enum AgentState
    {
        Running,
        Stopped
    }

    public AgentState State { get; set; } = AgentState.Stopped;
    public string SystemInstruction { get; set; }
    public List<Function> Tools { get; set; } = [];
    public List<IStep> History { get; set; } = [];
    public string Model { get; set; } = "gemini-3.8-flash";

    public Agent(InteractionsRepository interactionsRepository, FunctionService functionService)
    {
        _interactionsRepository = interactionsRepository;
        _functionService = functionService;
    }

    public void AddTool(Function tool)
    {
        Tools.Add(tool);
    }

    public void AddSystemInstruction(string instruction)
    {
        if (string.IsNullOrWhiteSpace(SystemInstruction))
        {
            SystemInstruction = instruction;
        }
        else
        {
            SystemInstruction += "\n" + instruction;
        }
    }

    public async Task<ModelOutputStep> StartAgent(string prompt)
    {
        if (AgentState.Running == State)
        {
            throw new InvalidOperationException("Agent is already running.");
        }

        State = AgentState.Running;

        var initial = new UserInputStep
        {
            Content = new List<Content> { new Content { Text = prompt } }
        };

        History.Add(initial);

        var request = new InteractionRequestBody
        {
            Input = History,
            Model = Model,
            SystemInstruction = SystemInstruction,
            Tools = Tools,
            Store = false
        };

        var interaction = await _interactionsRepository.Create(request);

        while (true)
        {
            if (interaction == null)
            {
                break;
            }

            var steps = interaction.Steps ?? [];
            History.AddRange(steps);

            foreach (var step in steps)
            {
                switch (step)
                {
                    case ModelOutputStep modelOutput:
                        return modelOutput;

                    case FunctionCallStep functionCall:
                        if (_functionService == null)
                        {
                            History.Add(FunctionResultStep.Failure(functionCall, "No function caller registered."));
                        }
                        var result = await _functionService!.GetFunctionResultStep(functionCall);
                        History.Add(result);
                        break;

                    case ThoughtStep thought:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Model thinking... ");

                        if (thought.Summary != null)
                        {
                            foreach (var content in thought.Summary.Where(c => c != null))
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("Thought: ");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine(content.Text);
                            }
                        }
                        break;

                    default:
                        var unhandledStep = new UserInputStep
                        {
                            Content = new List<Content> { new Content { Text = "The previous step was unable to be handled by the agent." } }
                        };
                        History.Add(unhandledStep);
                        break;
                }
            }

            var nextRequest = new InteractionRequestBody
            {
                Input = History,
                Model = Model,
                SystemInstruction = SystemInstruction,
                Tools = Tools.ToList(),
                Store = false
            };

            interaction = await _interactionsRepository.Create(nextRequest);
        }

        State = AgentState.Stopped;
        return new();
    }
}
