namespace CommandBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public string Pattern { get; }
        public string Description { get; }
        public bool ShowInMenu { get; }
        public string? Example { get; }
        public int GroupNumber { get; }
        public int Order { get; }

        public CommandAttribute(
            string pattern, 
            string description, 
            bool showInMenu = false, 
            string? example = null,
            int groupNumber = 99,
            int order = 999)
        {
            Pattern = pattern;
            Description = description;
            ShowInMenu = showInMenu;
            Example = example;
            GroupNumber = groupNumber;
            Order = order;
        }
    }
}
