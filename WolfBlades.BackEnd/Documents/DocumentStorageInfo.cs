namespace WolfBlades.BackEnd.Documents;

public class DocumentStorageInfo : IDataStorage<DocumentInfo>
{
    public string Description = "doc description";
    public string MarkdownBody = "";
    public int[] RelatedTasks = Array.Empty<int>(); //相关任务的ID列表
    public int[] RelatedUsers = Array.Empty<int>(); //相关的用户列表
    public string Title = "doc title";
    public int UploaderUserID = -1;
    public DateTime UploadTime = DateTime.Now;

    public void ReadFrom(DocumentInfo data)
    {
        Title = data.Title;
        Description = data.Description;
        MarkdownBody = data.MarkdownBody;
        UploaderUserID = data.UploaderUserID;
        UploadTime = data.UploadTime.ConvertToDateTime();
        RelatedTasks = data.RelatedTasks.ToList().ToArray();
        RelatedUsers = data.RelatedUsers.ToList().ToArray();
    }

    public void WriteTo(ref DocumentInfo data)
    {
        data.Title = Title;
        data.Description = Description;
        data.MarkdownBody = MarkdownBody;
        data.UploaderUserID = UploaderUserID;
        data.UploadTime = UploadTime.ConvertToString();
        data.RelatedTasks = RelatedTasks.ToList().ToArray();
        data.RelatedUsers = RelatedUsers.ToList().ToArray();
    }
}