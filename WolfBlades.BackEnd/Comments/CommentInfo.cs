namespace WolfBlades.BackEnd.Comments
{
    public struct CommentInfo
    {
        public int BindTaskID = -1;
        public string MarkdownBody = "";
        public int UploaderUserID = -1;
        public string UploadTime = "yyyy-mm-dd hh-mm-ss";

        public CommentInfo()
        {
        }
    }
}
