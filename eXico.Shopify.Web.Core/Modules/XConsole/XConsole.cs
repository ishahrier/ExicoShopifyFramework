using ColorConsole;
using ConsoleTables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{

    public class XConsole : IXConsole
    {
        #region Class Variables
        public const string SERVICE_CONSTROLLER = "X";
        public const string ARG_VALUE = "x_console";
        public const string CONSOLE_ENABL_KEY = "ConsoleEnabled";
        private readonly IWebHost _WebHost;
        private readonly IConfiguration _Config;
        private readonly ILogger<XConsole> _Logger;
        private readonly IConsoleWriter _Writer;
        private readonly List<string> _CommandList;
        public string Prompt = "Type a command:>>";

        public IWebHost WebHost => _WebHost;
        public IConfiguration Config => _Config;
        public ILogger<XConsole> Logger => _Logger;
        public IConsoleWriter Writer => _Writer;
        public List<string> CommandList => _CommandList;
        //public IConsoleWriter Writer => _Writer;
        #endregion

        public XConsole(IWebHost host)
        {
            _WebHost = host;
            _Config = _WebHost.Services.GetService<IConfiguration>();
            _Logger = _WebHost.Services.GetService<ILogger<XConsole>>();
            _Writer = new ColorConsole.ConsoleWriter();
            _CommandList = GetAllXCommands();
        }

        public void Start()
        {
            WriteSystemText(Environment.NewLine + "Starting X console.Please wait....");
            Thread.Sleep(2000);
            ;
            if (Config[CONSOLE_ENABL_KEY] == "1" || Config[CONSOLE_ENABL_KEY] == "yes")
            {
                WelcomeMessage();
                while (true)
                {
                    Writer.Write(Prompt, ConsoleColor.Black, ConsoleColor.White);
                    Writer.Write(" ");
                    string command = Console.ReadLine();
                    if (command == "quit" || command == "exit") Quit();
                    if (command == "clear" || command == "cls")
                    {
                        Console.Clear();
                        WelcomeMessage();
                    }
                    else
                    {
                        if (command == "help") command = "list-commands";
                        var commandClass = FindCommand(command);
                        if (commandClass != null)
                        {
                            IXCommand instance = null;
                            try
                            {
                                instance = CreateCommandInstance(commandClass);
                            }
                            catch (Exception ex)
                            {
                                WriteSystemText("Error creating command instance.");
                                Writer.WriteLine(ex.Message, ConsoleColor.DarkRed);
                                instance = null;
                            }
                            if (instance != null)
                            {
                                try
                                {
                                    instance.Run(this).GetAwaiter().GetResult();
                                }
                                catch (Exception ex)
                                {
                                    WriteSystemText("Unhandled exception thrown while running the command.");
                                    Writer.WriteLine(ex.Message, ConsoleColor.DarkRed);
                                }
                            }
                            else
                            {
                                WriteSystemText("Command instance is null. Cannot continue.");
                            }
                        }
                        else WriteSystemText($"Could not find the command {command}.");
                    }
                }
            }
            else
            {
                WriteSystemText("Console is disabled.");
                Quit();
            }
        }

        public void WriteTable(ConsoleTable table)
        {
            Writer.WriteLine("");
            table.Write(Format.Minimal);

        }

        public ConsoleTable CreateTable(IEnumerable<String> columns, bool enableCount = false)
        {

            return new ConsoleTable(new ConsoleTableOptions()
            {
                Columns = columns,
                EnableCount = enableCount
            });

        }

        public void WriteHelp(IXCommand command)
        {
            Writer.WriteLine($"HELP - {command.GetName()}", ConsoleColor.Cyan, ConsoleColor.Blue);
            Writer.WriteLine(command.GetDescription(), ConsoleColor.Gray);
        }

        public void WriteInfo(IXCommand command, string text, bool appendNewLine = true)
        {
            Write(command, text, appendNewLine, ConsoleColor.Cyan);
        }

        public void WriteSuccess(IXCommand command, string text, bool appendNewLine = true)
        {
            Write(command, text, appendNewLine, ConsoleColor.Green);
        }

        public void WriteError(IXCommand command, string text, bool appendNewLine = true)
        {
            Write(command, text, appendNewLine, ConsoleColor.Red);
        }

        public void WriteWarning(IXCommand command, string text, bool appendNewLine = true)
        {
            Write(command, text, appendNewLine, ConsoleColor.Yellow);
        }

        public void AskForInput(IXCommand command, string text, bool appendNewLine = false)
        {
            if (!text.EndsWith(": "))
            {
                if (text.Contains(":")) text += " ";
                else text += ": ";
            }
            Write(command, text, appendNewLine);
        }

        public void WriteException(IXCommand command, Exception ex)
        {
            Writer.WriteLine($"{command.GetName()} - Error Occurred ", ConsoleColor.White, ConsoleColor.Red);
            Writer.WriteLine(ex.Message, ConsoleColor.Red);
        }

        public string FindCommand(string commandName)
        {
            if (!string.IsNullOrEmpty(commandName))
            {
                commandName = $".{commandName.Replace("-", "")}";
                return _CommandList.Where(x => x.EndsWith(commandName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public IXCommand CreateCommandInstance(string commandClass)
        {

            try
            {
                var type = Type.GetType(commandClass);
                if (type == null)
                {
                    var asmblyName = Assembly.GetEntryAssembly().GetName().Name;
                    var fullName = Assembly.CreateQualifiedName(asmblyName, commandClass);
                    type = Type.GetType(fullName);
                }
                return (IXCommand)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #region Internal

        private void Write(IXCommand command, string text, bool appendNewLine = true, ConsoleColor color = ConsoleColor.White)
        {
            if (appendNewLine) Writer.WriteLine(text, color);
            else Writer.Write(text, color);
        }

        private List<string> GetAllXCommands()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                 .Where(x => typeof(IXCommand).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                 .Select(x => x.FullName).OrderBy(x => x).ToList();
        }

        private void Quit()
        {
            WriteSystemText(Environment.NewLine + "Quiting...");
            WebHost.Dispose();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private void WriteSystemText(string text)
        {
            Writer.WriteLine(text, ConsoleColor.Yellow, ConsoleColor.Magenta);
        }

        private void WelcomeMessage()
        {
            Writer.WriteLine("welcome to ", ConsoleColor.Magenta);
            string msg = @"__  __  ___                      _      
\ \/ / / __\___  _ __  ___  ___ | | ___ 
 \  / / /  / _ \| '_ \/ __|/ _ \| |/ _ \
 /  \/ /___ (_) | | | \__ \ (_) | |  __/
/_/\_\____/\___/|_| |_|___/\___/|_|\___|";
            Writer.WriteLine(msg + Environment.NewLine, ConsoleColor.Green);

        }
        #endregion
        public string GetPromptString()
        {
            return Prompt;
        }
        public void SetPromptString(string prompt)
        {
            Prompt = prompt;
        }

    }
}
