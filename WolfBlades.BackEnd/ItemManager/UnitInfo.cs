namespace WolfBlades.BackEnd.ItemManager;

public class UnitInfo : IItem
{
    /// <summary>
    ///     单位名称
    /// </summary>
    public string Name { get; set; } = "unit_name";

    /// <summary>
    ///     单位的显示名称
    /// </summary>
    public string DisplayName { get; set; } = "unit_recommend_display_name";

    /// <summary>
    ///     此单位所在的单位组 0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖
    /// </summary>
    public int UnitGroup { get; set; }

    /// <summary>
    ///     当前占用者的ID
    /// </summary>
    public int CurrentUserID { get; set; } = -1;

    /// <summary>
    ///     正在进行的任务的ID列表
    /// </summary>
    public List<int> InProgressTasks { get; set; } = new();

    /// <summary>
    ///     当前所有负责人员的ID列表
    /// </summary>
    public List<int> InChargeUsers { get; set; } = new();

    public int ID { get; set; } = -1;

    public void ReadFrom(IItem item)
    {
        if (item is not UnitInfo obj) return;

        Name = obj.Name;
        DisplayName = obj.DisplayName;
        UnitGroup = obj.UnitGroup;
        CurrentUserID = obj.CurrentUserID;
        InProgressTasks = new List<int>(InProgressTasks);
        InChargeUsers = new List<int>(InChargeUsers);
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}