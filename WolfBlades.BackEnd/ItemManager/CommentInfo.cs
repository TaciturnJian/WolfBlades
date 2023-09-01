namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     评论信息
/// </summary>
public class CommentInfo : IItem
{
    /// <summary>
    ///     绑定任务的ID
    /// </summary>
    public int BindTaskID { get; set; } = -1;

    /// <summary>
    ///     评论的Markdown内容
    /// </summary>
    public string MarkdownBody { get; set; } = "";

    /// <summary>
    ///     上传者ID
    /// </summary>
    public int UploaderUserID { get; set; } = -1;

    /// <summary>
    ///     上传时间
    /// </summary>
    public string UploadTime { get; set; } = TinyConverterExtension.ConvertToString(DateTime.Now);

    public int ID { get; set; }

    public void ReadFrom(IItem item)
    {
        if (item is not CommentInfo obj) return;

        BindTaskID = obj.BindTaskID;
        MarkdownBody = obj.MarkdownBody;
        UploaderUserID = obj.UploaderUserID;
        UploadTime = TinyConverterExtension.ConvertToString(obj.UploadTime.ConvertToDateTime());
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}