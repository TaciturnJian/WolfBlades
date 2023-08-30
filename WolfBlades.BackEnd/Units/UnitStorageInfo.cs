namespace WolfBlades.BackEnd.Units;

public class UnitStorageInfo : IDataStorage<UnitInfo>
{
    public int CurrentUserID = -1; //当前占用者的ID
    public string DisplayName = "unit_recommend_display_name";
    public List<int> InChargeUsers = new(); //所有负责人员的ID
    public List<int> InProgressTasks = new(); //当前未完成的任务ID
    public string Name = "unit_name";
    public int UnitGroup; //0未知 1英雄 2工程 3步兵 4哨兵 5无人机 6飞镖

    public void ReadFrom(UnitInfo data)
    {
        Name = data.Name;
        DisplayName = data.DisplayName;
        UnitGroup = data.UnitGroup;
        CurrentUserID = data.CurrentUserID;
        InProgressTasks = data.InProgressTasks.CopyIntArray().ToList();
        InChargeUsers = data.InChargeUsers.CopyIntArray().ToList();
    }

    public void WriteTo(ref UnitInfo data)
    {
        data.Name = Name;
        data.DisplayName = DisplayName;
        data.UnitGroup = UnitGroup;
        data.CurrentUserID = CurrentUserID;
        data.InProgressTasks = InProgressTasks.ToArray();
        data.InChargeUsers = InChargeUsers.ToArray();
    }
}