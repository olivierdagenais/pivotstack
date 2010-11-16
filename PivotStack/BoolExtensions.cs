namespace PivotStack
{
    public static class BoolExtensions
    {
        public static string YesNo (this bool input)
        {
            return input ? "yes" : "no";
        }
    }
}
