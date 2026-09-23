namespace FantasAIFootball.Tasks;

public interface ITask
{
    public string Name { get; }
    public Task Execute();
}
