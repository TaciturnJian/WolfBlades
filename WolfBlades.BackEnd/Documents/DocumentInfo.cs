namespace WolfBlades.BackEnd.Documents;

public struct DocumentInfo
{
    public string Title = "doc title";
    public string Description = "doc description";
    public string MarkdownBody = "";
    public int UploaderUserID = -1;
    public string UploadTime = "yyyy-mm-dd hh-mm-ss";
    public int[] RelatedTasks = Array.Empty<int>(); //相关任务的ID列表
    public int[] RelatedUsers = Array.Empty<int>(); //相关的用户列表

    public DocumentInfo()
    {
    }
}