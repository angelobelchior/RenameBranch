using System;
using System.Threading.Tasks;

namespace RenameBranch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pat = "<<Personal Access Token>>";
            
            var repositoryUrl = "https://github.com/<<user>>/<<repo>>";
            var fromBranchName = "master";
            var toBranchName = "main";

            var github = new GitHub.GitHub(pat, repositoryUrl, fromBranchName, toBranchName);
            await github.Execute();

            Console.ReadLine();
        }
    }
}