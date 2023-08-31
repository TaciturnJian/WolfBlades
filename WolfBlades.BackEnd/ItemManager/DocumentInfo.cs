namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     文档信息
/// </summary>
public class DocumentInfo : IItem
{
    /// <summary>
    ///     文档描述
    /// </summary>
    public string Description = "document_description";

    /// <summary>
    ///     文档的Markdown内容
    /// </summary>
    public string MarkdownBody = "";

    /// <summary>
    ///     文档相关的任务
    /// </summary>
    public List<int> RelatedTasks = new(); //相关任务的ID列表

    /// <summary>
    ///     文档相关的用户
    /// </summary>
    public List<int> RelatedUsers = new(); //相关的用户列表

    /// <summary>
    ///     文档标题
    /// </summary>
    public string Title = "document_title";

    /// <summary>
    ///     文档上传者的ID
    /// </summary>
    public int UploaderUserID = -1;

    /// <summary>
    ///     文档的上传时间
    /// </summary>
    public string UploadTime = TinyConverterExtension.ConvertToString(DateTime.Now);

    public int ID { get; set; }

    public void ReadFrom(IItem item)
    {
        if (item is not DocumentInfo obj) return;

        Description = obj.Description;
        MarkdownBody = obj.MarkdownBody;
        RelatedTasks = new List<int>(obj.RelatedTasks);
        RelatedUsers = new List<int>(obj.RelatedUsers);
        Title = obj.Title;
        UploaderUserID = obj.UploaderUserID;
        UploadTime = TinyConverterExtension.ConvertToString(obj.UploadTime.ConvertToDateTime());
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}