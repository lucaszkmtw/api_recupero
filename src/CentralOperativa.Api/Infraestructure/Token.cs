
namespace CentralOperativa.Infraestructure
{
    public sealed class Token
    {
        private readonly string key;
        private readonly string value;

        public Token(string key, string value)
        {
            this.key = key;
            this.value = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string Key { get { return key; } }
        public string Value { get { return value; } }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Key, Value);
        }
    }
}
