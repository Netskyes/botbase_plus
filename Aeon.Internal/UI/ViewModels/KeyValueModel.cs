namespace Aeon.Internal.UI
{
    public sealed class KeyValueModel
    {
        public object Key { get; set; }
        public object Value { get; set; }

        public KeyValueModel(object key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
