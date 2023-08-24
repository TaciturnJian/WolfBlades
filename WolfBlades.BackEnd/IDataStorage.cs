namespace WolfBlades.BackEnd;

public interface IDataStorage<T> where T : struct
{
    void ReadFrom(T data);
    void WriteTo(ref T data);
}