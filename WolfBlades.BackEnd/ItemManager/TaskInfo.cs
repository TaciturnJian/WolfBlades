namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     任务信息
/// </summary>
public class TaskInfo : IItem
{
    /// <summary>
    ///     绑定的单位的ID
    /// </summary>
    public int BindUnitID { get; set; } = -1;

    /// <summary>
    ///     任务预定期限
    /// </summary>
    public DateTime DeadLine { get; set; } = DateTime.Now;

    /// <summary>
    ///     任务描述
    /// </summary>
    public string Description { get; set; } = "pretty short description or empty";

    /// <summary>
    ///     绑定的文档的ID
    /// </summary>
    public int DocumentID { get; set; } = -1; //任务文档的ID

    /// <summary>
    ///     任务的结束时间
    /// </summary>
    public DateTime EndTime { get; set; } = DateTime.Now;

    /// <summary>
    ///     负责人列表
    /// </summary>
    public List<int> InChargeUsers { get; set; } = new(); //所有负责人员的ID

    /// <summary>
    ///     任务名称
    /// </summary>
    public string Name { get; set; } = "task_name";

    /// <summary>
    ///     任务进度 [0,1]
    /// </summary>
    public float Progress { get; set; }

    /// <summary>
    ///     任务的开始时间
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    public int ID { get; set; } = -1;

    public void ReadFrom(IItem item)
    {
        if (item is not TaskInfo obj) return;

        BindUnitID = obj.BindUnitID;
        DeadLine = obj.DeadLine;
        Description = obj.Description;
        DocumentID = obj.DocumentID;
        EndTime = obj.EndTime;
        InChargeUsers = new List<int>(obj.InChargeUsers);
        Name = obj.Name;
        Progress = obj.Progress;
        StartTime = obj.StartTime;
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}