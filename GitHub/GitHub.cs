using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RenameBranch.GitHub
{
    public class GitHub
    {
        private readonly Rest _rest;

        private readonly string _urlBase;
        private readonly string _fromBranchName;
        private readonly string _toBranchName;
        public GitHub(string pat, string repositoryUrl, string fromBranchName, string toBranchName)
        {
            if (string.IsNullOrWhiteSpace(pat))
                throw new ArgumentException($"'{nameof(pat)}' cannot be null or whitespace", nameof(pat));

            if (string.IsNullOrEmpty(repositoryUrl))
                throw new ArgumentException($"'{nameof(repositoryUrl)}' cannot be null or empty", nameof(repositoryUrl));

            if (string.IsNullOrEmpty(fromBranchName))
                throw new ArgumentException($"'{nameof(fromBranchName)}' cannot be null or empty", nameof(fromBranchName));

            if (string.IsNullOrEmpty(toBranchName))
                throw new ArgumentException($"'{nameof(toBranchName)}' cannot be null or empty", nameof(toBranchName));

            this._fromBranchName = fromBranchName;
            this._toBranchName = toBranchName;

            this._urlBase = this.GetUrlBase(repositoryUrl);

            this._rest = new Rest(pat);
        }

        public async Task Execute()
        {
            var branchs = await this.ListBranchs();
            if (branchs.Any(b => b.Name == this._toBranchName))
            {
                Console.WriteLine($"Already exists a branch with name {this._toBranchName}");
                return;
            }

            var from = branchs.FirstOrDefault(b => b.Name == this._fromBranchName);
            if (from is null)
            {
                Console.WriteLine($"There's no branch with name {this._fromBranchName}");
                return;
            }

            var to = await this.CreateNewBanch(from.Object.SHA);
            Console.WriteLine(to.Ref);

            await this.SetNewBranchAsDefault();
            await this.DeleteOldBranch();
        }

        private async Task<IEnumerable<Models.Branch>> ListBranchs()
        {
            var url = $"{this._urlBase}/git/refs/heads";
            var branchs = await this._rest.Get<IEnumerable<Models.Branch>>(url);
            return branchs;
        }

        private async Task<Models.Branch> CreateNewBanch(string shaMaster)
        {
            var url = $"{this._urlBase}/git/refs";
            var request = new { @ref = $"refs/heads/{this._toBranchName}", sha = shaMaster };
            var branch = await this._rest.Post<Models.Branch>(url, request);
            return branch;
        }

        private async Task SetNewBranchAsDefault()
        {
            var repositoryName = this._urlBase.Split("/").LastOrDefault();
            var request = new { name = repositoryName, default_branch = this._toBranchName };
            await this._rest.Patch(this._urlBase, request);
        }

        private async Task DeleteOldBranch()
        {
            var url = $"{this._urlBase}/git/refs/heads/{this._fromBranchName}";
            await this._rest.Delete(url);
        }

        private string GetUrlBase(string repositoryUrl)
        {
            var url = repositoryUrl;
            if (url.EndsWith("\\") || url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);

            var parts = url.Split("/");
            var username = parts[parts.Length - 2];
            var repositoryName = parts[parts.Length - 1];
            return $"https://api.github.com/repos/{username}/{repositoryName}";
        }
    }
}