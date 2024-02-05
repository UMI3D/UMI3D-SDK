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

    }
}