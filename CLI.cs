using System.CommandLine;

namespace Ally
{
    internal static class CLI
    {
        private const string Description = 
            "Simple tool for managing unix-like aliases on windows." +
            "\n  Without arguments, `ally` prints the list of aliases in the reusable form `ally <name> <value>` on standard output." +
            "\n  Otherwise, if <value> is given, an alias is defined for <name> and <value>," +
            "\n  and if <value> is not given, any existing alias corresponding to <name> is removed." +
            "\n  By default, all parameters given when calling alias are forwaded to <value>." +
            "\n  To disable parameters forwarding, append %! at the end of <value> when defining the alias." +
            "\n  Additionally, you may use a preceding ! to escape enviroment variables in aliases." +
            "\n  Ex. ally show-profile \"echo !%USERPROFILE!%\"" +
            "\n  Now, the enviroment variable will be evaluated when the alias is called."
            ;

        private static async Task<int> Main(string[] args)
        {
            Argument<string?> argName = new(
                name: "name",
                description: "The alias to be defined for given value.",
                getDefaultValue: () => null
            );
            Argument<string?> argValue = new(
                name: "value",
                description: "Required value to be binded to given alias.",
                getDefaultValue: () => null
            );

            Option<string> optSearch = new(
                name: "--search",
                description: "Display all aliases that contain <query> in the reusable form `ally <name> <value>`"
             ) { ArgumentHelpName = "query" };
            optSearch.AddAlias("-s");

            Option<bool> flagClear = new(
                name: "--clear",
                description: "Clear all currently set aliases."
            );
            flagClear.AddAlias("-c");

            RootCommand console = new(Description) {
                argName, 
                argValue,
                optSearch,
                flagClear
            };

            console.SetHandler(Handler, argName, argValue, optSearch, flagClear);
            
            return await console.InvokeAsync(args);
        }

        private static void Handler(string? name, string? value, string? query, bool clear)
        {
            if (clear == true) Ally.ClearAliases(); // Clear
            else if (query != null) DisplayAliases(Ally.GetAliases(query)); // Search
            else if (name == null) DisplayAliases(Ally.IterAliases()); // Display
            else if (value == null) Ally.DeleteAlias(name); // Delete
            else Ally.RegisterAlias(new(name, value)); // Register
        }

        private static void DisplayAliases(IEnumerable<Alias> aliases) {
            foreach (Alias alias in aliases) Console.WriteLine($"ally {alias.Name} {alias.Value}");
        }
    }
}