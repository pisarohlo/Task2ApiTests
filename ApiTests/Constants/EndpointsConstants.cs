namespace ApiTests.Constants
{
    public class EndpointsConstants
    {
        public static string GetDeleteRepository(string owner, string repoName) => $"/repos/{owner}/{repoName}";
        public const string CreateRepository = "user/repos";
    }
}
