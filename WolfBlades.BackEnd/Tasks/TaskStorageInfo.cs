namespace WolfBlades.BackEnd.Tasks;

public class TaskStorageInfo : IDataStorage<TaskInfo>
{
    public int BindUnitID = -1; //绑定的单位的ID
    public DateTime DeadLine = DateTime.Now;
    public string Description = "pretty short description or empty";
    public int DocumentID = -1; //任务文档的ID
    public DateTime EndTime = DateTime.Now;
    public int[] InChargeUsers = Array.Empty<int>(); //所有负责人员的ID
    public string Name = "task_name";
    public float Progress; //任务进度 [0,1]
    public DateTime StartTime = DateTime.Now;

    public void ReadFrom(TaskInfo data)
    {
        Name = data.Name;
        Description = data.Description;
        DocumentID = data.DocumentID;
        BindUnitID = data.BindUnitID;
        InChargeUsers = data.InChargeUsers.ToList().ToArray();
        Progress = data.Progress;
        StartTime = data.StartTime.ConvertToDateTime();
        DeadLine = data.DeadLine.ConvertToDateTime();
        EndTime = data.EndTime.ConvertToDateTime();
    }

    public void WriteTo(ref TaskInfo data)
    {
        data.Name = Name;
        data.Description = Description;
        data.DocumentID = DocumentID;
        data.BindUnitID = BindUnitID;
        data.InChargeUsers = InChargeUsers;
        data.Progress = Progress;
        data.StartTime = StartTime.ConvertToString();
        data.DeadLine = DeadLine.ConvertToString();
        data.EndTime = EndTime.ConvertToString();
    }
}