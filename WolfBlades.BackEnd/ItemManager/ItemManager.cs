using System.Text;
using Fleck;
using Newtonsoft.Json;

namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     非常简单的项目管理器实现
/// </summary>
/// <typeparam name="T"></typeparam>
public class ItemManager<T> : IItemManager
    where T : IItem, new()
{
    protected Dictionary<int, T> Database = new();
    public int ID { get; set; } = -1;

    public virtual void ReadFrom(IItem item)
    {
        ID = item.ID;

        var manager = item as IItemManager;
        manager?.SelectItems(itemInManager =>
        {
            AppendItem(itemInManager);
            return false;
        });
    }

    public virtual void ReadFrom(string content)
    {
        var manager = JsonConvert.DeserializeObject<ItemManager<T>>(content);
        if (manager != null) ReadFrom(manager);
    }

    public string Name { get; set; } = "ItemManager";

    public virtual IItem GetItem(int id)
    {
        var data = new T();
        if (!Database.ContainsKey(id))
        {
            data.ID = -1;
            return data;
        }

        data.ReadFrom(Database[id]);
        WhenQueryItem?.Invoke(this, new ManagerEventArgs { Item = Database[id] });
        return data;
    }

    public virtual int AppendItem(IItem item)
    {
        var id = 0;
        for (; id <= Database.Count; id++)
            if (!Database.ContainsKey(id))
                break;

        item.ID = id;
        var data = new T();
        data.ReadFrom(item);

        Database.Add(id, data);
        WhenAppendItem?.Invoke(this, new ManagerEventArgs { Item = Database[id] });
        return id;
    }

    public virtual bool RemoveItem(int id)
    {
        if (!Database.ContainsKey(id)) return true;

        Database.Remove(id);
        var item = Database[id];
        item.ID = -1;
        WhenRemoveItem?.Invoke(this, new ManagerEventArgs { Item = item });
        return true;
    }

    public virtual bool UpdateItem(int id, IItem item)
    {
        if (!Database.ContainsKey(id)) return false;

        Database[id].ReadFrom(item);
        WhenUpdateItem?.Invoke(this, new ManagerEventArgs { Item = Database[id] });
        return true;
    }

    public virtual int[] SelectItems(Func<IItem, bool> selector)
    {
        var result = new List<int>();
        foreach (var (id, item) in Database.Where(pair => selector(pair.Value)))
        {
            result.Add(id);
            WhenSelectItems?.Invoke(this, new ManagerEventArgs { Item = item });
        }

        return result.ToArray();
    }

    public virtual bool SaveDataToFile(string filepath)
    {
        try
        {
            if (File.Exists(filepath)) File.Delete(filepath);

            using var fs = File.Create(filepath);
            fs.Write(Encoding.UTF8.GetBytes(Database.ToJsonString(true)));
            return true;
        }
        catch (Exception ex)
        {
            FleckLog.Error("无法将数据保存到文件", ex);
            return false;
        }
    }

    public virtual bool ReadDataFromFile(string filepath)
    {
        try
        {
            if (!File.Exists(filepath))
            {
                File.Create(filepath);
                return true;
            }

            var text = Encoding.UTF8.GetString(File.ReadAllBytes(filepath));
            var database = JsonConvert.DeserializeObject<Dictionary<int, T>>(text);
            if (database == null) return false;
            Database = database;
            return true;
        }
        catch (Exception ex)
        {
            FleckLog.Error("无法从文件中读取数据", ex);
            return false;
        }
    }

    public event Action<IItemManager, ManagerEventArgs>? WhenQueryItem;
    public event Action<IItemManager, ManagerEventArgs>? WhenAppendItem;
    public event Action<IItemManager, ManagerEventArgs>? WhenRemoveItem;
    public event Action<IItemManager, ManagerEventArgs>? WhenUpdateItem;
    public event Action<IItemManager, ManagerEventArgs>? WhenSelectItems;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public class TechGroupInfo
{
}

public class UnitGroupInfo
{
}