using System.ComponentModel;
using System.Drawing;
using System.Net.Mime;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Results;

namespace Guardian.Commands
{
    public class NoteCommands : CommandGroup
    {
        [Command("autocomplete-test-command")]
        public Task<Result> Create([AutocompleteProvider("autocomplete::reason")] string content)
        {
            return Task.FromResult(Result.FromSuccess());
        }
    }
}