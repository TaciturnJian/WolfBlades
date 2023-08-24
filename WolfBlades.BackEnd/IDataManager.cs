namespace WolfBlades.BackEnd;

public interface IDataManager<out TStorage, TQuery>
    where TStorage : class, IDataStorage<TQuery>
    where TQuery : struct
{
    bool QueryInfoByID(int id, ref TQuery info);

    int[] QueryIDListBySelector(Func<TStorage, bool> selector);

    int AddItem(TQuery info);

    bool RemoveItem(int id);

    bool UpdateItem(int id, TQuery info);

    bool SaveDataToFile(string path);

    bool ReadDataFromFile(string path);
}