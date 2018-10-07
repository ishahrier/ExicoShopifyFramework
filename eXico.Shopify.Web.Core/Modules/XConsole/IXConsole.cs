using ColorConsole;
using ConsoleTables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public interface IXConsole
    {
        void AskForInput(IXCommand command, string text, bool appendNewLine = false);
        void WriteInfo(IXCommand command, string text, bool appendNewLine = true);
        void WriteSuccess(IXCommand command, string text, bool appendNewLine = true);
        void WriteError(IXCommand command, string text, bool appendNewLine = true);
        void WriteException(IXCommand command, Exception ex);
        void WriteWarning(IXCommand command, string text, bool appendNewLine = true);
        void WriteHelp(IXCommand command);

        void Start();

        ConsoleTable CreateTable(IEnumerable<String> columns, bool enableCount = false);
        void WriteTable(ConsoleTable table);
        IWebHost WebHost { get; }
        IConfiguration Config { get; }
        ILogger<XConsole> Logger { get; }
        IConsoleWriter Writer { get; }
        List<string> CommandList { get; }

        string GetPromptString();
        void SetPromptString(string prompt);
    }
}