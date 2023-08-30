namespace WolfBlades.BackEnd.Units;

public class UnitInfoManager : DataManager<UnitStorageInfo, UnitInfo>
{
    public bool AppendTaskInUnitProgressTask(int unitID, int taskID)
    {
        if (!Database.ContainsKey(unitID)) return false;

        var user = Database[unitID];
        if (!user.InProgressTasks.Contains(taskID)) user.InProgressTasks.Add(taskID);

        return true;
    }

    public bool RemoveTaskInUnitProgressTask(int unitID, int taskID)
    {
        return Database.ContainsKey(unitID) && Database[unitID].InProgressTasks.Remove(taskID);
    }

    public bool UpdateTaskListForUnit(int unitID, int[] taskList)
    {
        if (!Database.ContainsKey(unitID)) return false;

        var user = Database[unitID];
        user.InChargeUsers.Clear();
        foreach (var task in taskList) user.InChargeUsers.Add(task);

        return true;
    }
}