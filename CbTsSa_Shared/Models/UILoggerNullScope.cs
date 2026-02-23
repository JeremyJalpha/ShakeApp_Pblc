namespace CbTsSa_Shared.Models
{
    public class UILoggerNullScope : IDisposable
    {
        public static UILoggerNullScope Instance { get; } = new();

        private UILoggerNullScope() { }

        public void Dispose() { }
    }
}
