using System;
using System.Collections.Generic;

namespace b7.Xabbo.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string CommandName { get; }
        public IReadOnlyList<string> Aliases { get; }

        public string Usage { get; set; }

        public CommandAttribute(string commandName, params string[] aliases)
        {
            CommandName = commandName;
            Aliases = aliases;
        }
    }
}
