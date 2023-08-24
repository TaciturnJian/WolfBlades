namespace WolfBlades.BackEnd.Comments;

public class CommentStorageInfo : IDataStorage<CommentInfo>
{
    public int BindTaskID = -1;
    public string MarkdownBody = "";
    public int UploaderUserID = -1;
    public DateTime UploadTime = DateTime.Now;

    public void ReadFrom(CommentInfo data)
    {
        BindTaskID = data.BindTaskID;
        MarkdownBody = data.MarkdownBody;
        UploaderUserID = data.UploaderUserID;
        UploadTime = data.UploadTime.ConvertToDateTime();
    }

    public void WriteTo(ref CommentInfo data)
    {
        data.BindTaskID = BindTaskID;
        data.MarkdownBody = MarkdownBody;
        data.UploaderUserID = UploaderUserID;
        data.UploadTime = UploadTime.ConvertToString();
    }
}