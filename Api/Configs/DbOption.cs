namespace Api.Configs
{
    public class DbOption
    {
        public const string SectionName = "ConnectionStrings";
        public string DbConnectionString { get; set; } = string.Empty;

    }
}
