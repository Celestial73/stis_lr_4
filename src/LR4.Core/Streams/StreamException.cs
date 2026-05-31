namespace LR4.Core.Streams;

public sealed class EndOfStreamException : Exception
{
    public EndOfStreamException() : base("End of stream reached.") { }
    public EndOfStreamException(string message) : base(message) { }
}
