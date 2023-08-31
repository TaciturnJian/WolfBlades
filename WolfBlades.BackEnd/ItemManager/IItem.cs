namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
/// 代表一个可以管理的项目，内部存储一个ID
/// </summary>
public interface IItem
{
    /// <summary>
    ///     项目在系统中的ID，-1代表不在系统中
    /// </summary>
    int ID { get; set; }

    /// <summary>
    ///     从另一个项目中读取信息保存到此项目中
    /// </summary>
    /// <param name="item"></param>
    void ReadFrom(IItem item);

    /// <summary>
    ///     从字符串中读取信息保存到此项目中
    /// </summary>
    /// <param name="content"></param>
    void ReadFrom(string content);
}