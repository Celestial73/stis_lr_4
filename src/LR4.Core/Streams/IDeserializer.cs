namespace LR4.Core.Streams;

public interface IDeserializer<T>
{
    T Deserialize(string line);
}

public interface ISerializer<T>
{
    string Serialize(T item);
}
