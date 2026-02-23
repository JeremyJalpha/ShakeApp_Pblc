using System.Text.RegularExpressions;

namespace CommandBot.Interfaces
{
    public interface IPatternCommand : ICommand
    {
        void Initialize(Match match);
    }
}
