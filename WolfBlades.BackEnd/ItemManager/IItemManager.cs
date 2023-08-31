namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     代表一个项目管理器，存储所有项目并提供到文件的储存方法
/// </summary>
public interface IItemManager : IItem
{
    string Name { get; set; }

    /// <summary>
    ///     提供ID，获取项目
    /// </summary>
    /// <param name="id">项目的ID</param>
    /// <returns>当返回的项目的ID为-1时说明失败，否则成功</returns>
    IItem GetItem(int id);

    /// <summary>
    ///     添加项目，自动为项目的ID赋值
    /// </summary>
    /// <param name="item">要添加的内容</param>
    /// <returns>项目的ID</returns>
    int AppendItem(IItem item);

    /// <summary>
    ///     移除项目，自动将被移除的项目的ID赋值为-1
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <returns>项目是否存在</returns>
    bool RemoveItem(int id);

    /// <summary>
    ///     提供ID和内容，更新项目
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <param name="item">项目内容</param>
    /// <returns>项目是否存在</returns>
    bool UpdateItem(int id, IItem item);

    /// <summary>
    ///     从所有项目中选择出特定项目
    /// </summary>
    /// <param name="selector">选择器</param>
    /// <returns></returns>
    int[] SelectItems(Func<IItem, bool> selector);

    /// <summary>
    ///     将数据保存到文件中
    /// </summary>
    /// <param name="filepath">文件路径</param>
    /// <returns>是否成功保存</returns>
    bool SaveDataToFile(string filepath);

    /// <summary>
    ///     从文件中读取数据
    /// </summary>
    /// <param name="filepath">文件路径</param>
    /// <returns>是否成功读取</returns>
    bool ReadDataFromFile(string filepath);

    /// <summary>
    ///     在查询项目时被调用
    /// </summary>
    event Action<IItemManager, ManagerEventArgs>? WhenQueryItem;

    /// <summary>
    ///     在添加项目时被调用
    /// </summary>
    event Action<IItemManager, ManagerEventArgs>? WhenAppendItem;

    /// <summary>
    ///     在移除项目时被调用
    /// </summary>
    event Action<IItemManager, ManagerEventArgs>? WhenRemoveItem;

    /// <summary>
    ///     在更新项目时被调用
    /// </summary>
    event Action<IItemManager, ManagerEventArgs>? WhenUpdateItem;

    /// <summary>
    ///     在选择项目时被调用
    /// </summary>
    event Action<IItemManager, ManagerEventArgs>? WhenSelectItems;
}