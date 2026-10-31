using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(ChangeTitle), TaskConstants.TaskCommand.ChangeTitle)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.ChangeTitle)]
  public sealed record ChangeTitle : TaskCommand
  {
    [Id(0)]
    public required string Title { get; init; }

    [JsonConstructor]
    internal ChangeTitle() { }

    private const string _errorTitle = "Invalid 'ChangeTitle' command";
    private const string _errorDetail = "The title cannot be changed.";

    public static DomainResult<ChangeTitle> Create(string? title)
    {
      return TaskExtensions
        .ValidateTitle(title, _errorTitle, _errorDetail, nameof(Title))
        .Pipe(validTitle => DomainResult.Success(new ChangeTitle { Title = validTitle }));
    }
  }
}
