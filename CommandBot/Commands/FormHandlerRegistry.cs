using CommandBot.Commands.FormHandlers;

namespace CommandBot.Commands
{
    /// <summary>
    /// Registry for web form handlers. Allows dynamic registration of form types.
    /// </summary>
    public class FormHandlerRegistry
    {
        private readonly Dictionary<string, IFormHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Register a form handler
        /// </summary>
        public void Register(IFormHandler handler)
        {
            _handlers[handler.FormType] = handler;
        }

        /// <summary>
        /// Get a handler by form type
        /// </summary>
        public IFormHandler? GetHandler(string formType)
        {
            _handlers.TryGetValue(formType, out var handler);
            return handler;
        }

        /// <summary>
        /// Get all registered form types
        /// </summary>
        public IEnumerable<string> GetRegisteredFormTypes()
        {
            return _handlers.Keys;
        }
    }
}
