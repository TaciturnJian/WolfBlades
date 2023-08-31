namespace WolfBlades.BackEnd.ItemManager;

public class CommentInfo : IItem
{
    /// <summary>
    ///     绑定任务的ID
    /// </summary>
    public int BindTaskID = -1;

    /// <summary>
    ///     评论的Markdown内容
    /// </summary>
    public string MarkdownBody = "";

    /// <summary>
    ///     上传者ID
    /// </summary>
    public int UploaderUserID = -1;

    /// <summary>
    ///     上传时间
    /// </summary>
    public string UploadTime = TinyConverterExtension.ConvertToString(DateTime.Now);

    public int ID { get; set; }

    public void ReadFrom(IItem item)
    {
        if (item is not CommentInfo obj) return;

        BindTaskID = obj.BindTaskID;
        MarkdownBody = obj.MarkdownBody;
        UploaderUserID = obj.UploaderUserID;
        UploadTime = obj.UploadTime;
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}