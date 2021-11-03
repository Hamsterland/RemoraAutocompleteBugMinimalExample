using FuzzySharp;
using Humanizer;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Autocomplete;

namespace Guardian.Commands;

public class ReasonAutocompleteProvider : IAutocompleteProvider
{
    private readonly IReadOnlySet<string> _reasons = new SortedSet<string>
    {
        "Test Content 1",
        "Test Content 2",
        "Test Content 3",
    };

    public ValueTask<IReadOnlyList<IApplicationCommandOptionChoice>> GetSuggestionsAsync(
        IReadOnlyList<IApplicationCommandInteractionDataOption> options, 
        string userInput,
        CancellationToken ct = default)
    {
        return new ValueTask<IReadOnlyList<IApplicationCommandOptionChoice>>
        (
            _reasons
                .OrderByDescending(n => Fuzz.Ratio(userInput, n))
                .Select(n => new ApplicationCommandOptionChoice(n.Humanize().Transform(To.TitleCase), n))
                .ToList()
        );
    }

    public string Identity => "autocomplete::reason";
}