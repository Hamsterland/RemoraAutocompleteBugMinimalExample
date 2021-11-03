using System.ComponentModel;
using System.Drawing;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Guardian.Commands;

public class NoteCommands : CommandGroup
{
    private readonly FeedbackService _feedbackService;

    public NoteCommands(FeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [Command("note")]
    [Description("creates a new note")]
    public async Task<Result> Create(
        [Description("the user to note")] IUser user,
        [Description("the note content")] [AutocompleteProvider("autocomplete::reason")] string note)
    {
        await _feedbackService.SendContextualContentAsync(note, Color.Olive);
        return Result.FromSuccess();
    }
}