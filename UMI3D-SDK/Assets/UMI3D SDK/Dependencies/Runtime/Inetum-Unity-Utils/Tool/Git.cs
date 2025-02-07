/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
namespace inetum.unityUtils
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class Git
    {
        static public async Task<string> GetBranchName()
        {
            string gitCommand = "git";
            string gitAddArgument = @"branch --show-current";
            string answer = null;

            await Command.ExecuteCommand(gitCommand, gitAddArgument, (s) => answer += s, (s) => answer += s);

            return answer;
        }

        static public async Task CommitAll(string CommitMessage, Action<string> output, Action<string> error)
        {
            string gitCommand = "git";
            string gitAddArgument = @"add .";
            string gitCommitArgument = $"commit -m \"{CommitMessage}\"";
            string gitPushArgument = @"push";

            await Command.ExecuteCommand(gitCommand, gitAddArgument, output, error);
            await Command.ExecuteCommand(gitCommand, gitCommitArgument, output, error);
            await Command.ExecuteCommand(gitCommand, gitPushArgument, output, error);
        }

        static public async Task CommitAllWithTag(string CommitMessage, string tagName, string tagMessage, Action<string> output, Action<string> error)
        {
            string gitCommand = "git";
            string gitAddArgument = @"add .";
            string gitCommitArgument = $"commit -m \"{CommitMessage}\"";
            string gitTagArgument = $"tag -a {tagName} -m \"{tagMessage}\"";
            string gitPushArgument = @"push --follow-tags";

            await Command.ExecuteCommand(gitCommand, gitAddArgument, output, error);
            await Command.ExecuteCommand(gitCommand, gitCommitArgument, output, error);
            await Command.ExecuteCommand(gitCommand, gitTagArgument, output, error);
            await Command.ExecuteCommand(gitCommand, gitPushArgument, output, error);
        }

        static public async Task<string> GetAllTag()
        {
            string gitCommand = "git";
            string gitAddArgument = @"tag";
            string answer = null;

            await Command.ExecuteCommand(gitCommand, gitAddArgument, (s) => answer += s, (s) => answer += s);

            return answer;
        }

        static public async Task<string> GetLastTag()
        {
            string gitCommand = "git";
            string gitAddArgument = @"describe --tags --abbrev=0";
            string answer = null;

            await Command.ExecuteCommand(gitCommand, gitAddArgument, (s) => answer += s, (s) => answer += s);

            return answer;
        }

        static public async Task<string> GetAllCommitSince(string tag)
        {
            string gitCommand = "git";
            string gitAddArgument = @"log "+tag+"..HEAD";
            string answer = null;

            await Command.ExecuteCommand(gitCommand, gitAddArgument, (s) => answer += s, (s) => answer += s);

            return answer;
        }

        static public async Task<List<Commit>> GetAllCommitSinceAndCast(string tag)
        {
            string gitCommand = "git";
            string gitAddArgument = @"log " + tag + "..HEAD";
            string answer = null;

            await Command.ExecuteCommand(gitCommand, gitAddArgument, (s) => answer += "\n" + s, (s) => answer += "\n"+ s);
            UnityEngine.Debug.Log(answer);
            //string[] lines = answer.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            //Commit current;
            List<Commit> result = new();
            string _regex = @"commit (?<id>[a-f0-9]+)\n(?:Merge:\s*(?<merge>[^\n]+)\n)?Author:\s*(?<author>[^\n]+)\nDate:\s*(?<date>[^\n]+)\n*[ ]*(?<title>[^\n]+)+\n(?<message>(?:    [^\n]*\n)*)";
            Regex regex = new(_regex);
            var matches = regex.Matches(answer);
            foreach(Match match in matches)
            {
                if (!match.Success)
                    continue;

                var commit = new Commit
                {
                    Id = match.Groups["id"].Value,
                    Merge = match.Groups["merge"].Value,
                    Author = match.Groups["author"].Value,
                    Date = match.Groups["date"].Value,
                    Title = match.Groups["title"].Value.Trim(),
                    Message = match.Groups["message"].Value.Trim()
                };

                result.Add(commit);
            }

            return result;
        }

        static string GetRemainingStringIfStartsWith(string input, string prefix)
        {
            if (input.StartsWith(prefix))
            {
                return input.Substring(prefix.Length);
            }
            else
            {
                return null; // or you can return string.Empty or any other indication
            }
        }

        public class Commit
        {
            public string Id { get; set; }
            public string Merge { get; set; }
            public string Author { get; set; }
            public string Date { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
        }

    }
}