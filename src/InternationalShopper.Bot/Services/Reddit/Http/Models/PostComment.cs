namespace InternationalShopper.Bot.Services.Reddit.Http.Models;

public record PostComment(string Body, string? FlairText, string Author)
{
    public PostComment(CommentData comment) : this(comment.Body, comment.FlairText, comment.Author) { }
}