namespace WolfBlades.BackEnd.Tasks;

public struct TaskInfo
{
    public string Name = "task_name";
    public string Description = "pretty short description or empty";
    public int DocumentID = -1; //任务文档的ID
    public int BindUnitID = -1; //绑定的单位的ID
    public int[] InChargeUsers = Array.Empty<int>(); //所有负责人员的ID
    public float Progress = 0.0f; //任务进度 [0,1]
    public string StartTime = "yyyy-mm-dd hh:mm:ss";
    public string DeadLine = "yyyy-mm-dd hh:mm:ss";
    public string EndTime = "'-' or 'yyyy-mm-dd hh:mm:ss'";

    public TaskInfo()
    {
    }
}