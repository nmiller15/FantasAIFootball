using System.Text;

namespace FantasAIFootball.Utilities;

public class TeeWriter(TextWriter first, TextWriter second) : TextWriter
{
    public override Encoding Encoding => first.Encoding;

    public override void Write(char value)
    {
        first.Write(value);
        second.Write(value);
    }

    public override void Write(char[] buffer, int index, int count)
    {
        first.Write(buffer, index, count);
        second.Write(buffer, index, count);
    }

    public override void Write(string? value)
    {
        first.Write(value);
        second.Write(value);
    }
}
