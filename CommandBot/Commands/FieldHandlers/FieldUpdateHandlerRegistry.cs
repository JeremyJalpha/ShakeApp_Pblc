namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Registry for user field update handlers. Allows dynamic registration of field types.
    /// </summary>
    public class FieldUpdateHandlerRegistry
    {
        private readonly Dictionary<string, IFieldUpdateHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Register a field update handler
        /// </summary>
        public void Register(IFieldUpdateHandler handler)
        {
            _handlers[handler.FieldName] = handler;
        }

        /// <summary>
        /// Get a handler by field name
        /// </summary>
        public IFieldUpdateHandler? GetHandler(string fieldName)
        {
            _handlers.TryGetValue(fieldName, out var handler);
            return handler;
        }

        /// <summary>
        /// Get all registered field names
        /// </summary>
        public IEnumerable<string> GetRegisteredFieldNames()
        {
            return _handlers.Keys;
        }
    }
}
