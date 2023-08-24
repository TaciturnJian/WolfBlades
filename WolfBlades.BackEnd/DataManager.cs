using System.Text;
using Fleck;
using Newtonsoft.Json;

namespace WolfBlades.BackEnd;

public interface IDataStorage<T> where T : struct
{
    void ReadFrom(T data);
    void WriteTo(ref T data);
}

public interface IDataManager<out TStorage, TQuery>
    where TStorage : class, IDataStorage<TQuery>
    where TQuery : struct
{
    bool QueryInfoByID(int id, ref TQuery info);

    int[] QueryIDListBySelector(Func<TStorage, bool> selector);

    int AddItem(TQuery info);

    bool RemoveItem(int id);

    bool UpdateItem(int id, TQuery info);
}

public class DataManager<TStorage, TQuery> : IDataManager<TStorage, TQuery>
    where TStorage : class, IDataStorage<TQuery>, new()
    where TQuery : struct
{
    protected Dictionary<int, TStorage> Database = new();

    public bool QueryInfoByID(int id, ref TQuery info)
    {
        if (!Database.ContainsKey(id)) return false;

        Database[id].WriteTo(ref info);
        return true;
    }

    public int[] QueryIDListBySelector(Func<TStorage, bool> selector)
    {
        return (from data in Database where selector(data.Value) select data.Key).ToArray();
    }

    public int AddItem(TQuery info)
    {
        var id = 0;
        for (; id <= Database.Count; id++)
            if (!Database.ContainsKey(id))
                break;

        var storage = new TStorage();
        storage.ReadFrom(info);
        return Database.TryAdd(id, storage) ? id : -1;
    }

    public bool RemoveItem(int id)
    {
        return Database.Remove(id);
    }

    public bool UpdateItem(int id, TQuery info)
    {
        if (!Database.ContainsKey(id)) return false;

        Database[id].ReadFrom(info);
        return true;
    }

    public bool SaveDataToFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);

            using var fs = File.Create(path);
            fs.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Database, Formatting.Indented)));

            return true;
        }
        catch (Exception ex)
        {
            FleckLog.Error("Cannot save data to file", ex);
            return false;
        }
    }

    public bool ReadDataFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                File.Create(path);
                return false;
            }

            var text = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            if (JsonConvert.DeserializeObject(text, typeof(Dictionary<int, TStorage>)) is not Dictionary<int, TStorage>
                dictionary) return false;

            Database = dictionary;
            return true;
        }
        catch (Exception ex)
        {
            FleckLog.Error("Cannot read data from file", ex);
            return false;
        }
    }
}