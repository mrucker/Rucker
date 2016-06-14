namespace Rucker.Data
{
    internal static class Hasher
    {
        internal static string Hash(string value)
        {
            return string.Intern(value);
        }
    }
}