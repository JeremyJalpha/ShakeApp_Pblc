namespace CbTsSa_Shared.DBModels
{
    public class Comment
    {
        public long CommentID { get; set; }
        public required string UserID { get; set; }
        public long SaleableID { get; set; }
        public string? CommentText { get; set; }
        public byte? Rating { get; set; }
        public long? ResponseToCommentID { get; set; }
        
        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Saleable Saleable { get; set; } = null!;
        public Comment? ResponseToComment { get; set; }
    }
}