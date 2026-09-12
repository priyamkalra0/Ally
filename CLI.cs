using System.CommandLine;

namespace Ally
{
    internal static class CLI
    {
        private const string Description = 
            "Simple tool for managing unix-like aliases on windows." +
            "\n  Without arguments, `ally` prints a list of aliases in the reusable form `ally <name> <value>` on standard output." +
            "\n  Otherwise, if both `<name>` & `<value>` are passed, an alias is defined binding `<name>` to `<value>`" +
            "\n  and if only `<name>` is passed, any existing alias corresponding to `<name>` is removed." +
            "\n  By default, all parameters given when calling an alias are forwarded to <value>." +
            "\n  To disable parameter forwarding, append %! at the end of <value> when defining the alias." +
            "\n  Additionally, you may use a preceding ! to escape environment variables in aliases." +
            "\n  Ex. ally show-profile \"echo !%USERPROFILE!%\"" +
            "\n  Now, the environment variable will be evaluated every time when the alias is called, not when it is defined."
            ;

        private static int Main(string[] args)
        {
            Argument<string?> argName = new("name") {
                Description = "The alias name.",
                DefaultValueFactory = _ => null
            };
            Argument<string?> argValue = new("value") {
                Description = "The value bound to the alias.",
                DefaultValueFactory = _ => null
            };

            Option<string> optSearch = new("--search", "-s") {
                Description = "Display all aliases that contain <query> in the reusable form `ally <name> <value>`",
                HelpName = "query"
            };

            Option<bool> flagClear = new("--clear", "-c") {
                Description = "Clear all currently set aliases."
            };

            RootCommand root = new(Description);
            root.Arguments.Add(argName);
            root.Arguments.Add(argValue);
            root.Options.Add(optSearch);
            root.Options.Add(flagClear);

            root.SetAction(parseResult =>
            {
                string? name = parseResult.GetValue(argName);
                string? value = parseResult.GetValue(argValue);
                string? query = parseResult.GetValue(optSearch);
                bool clear = parseResult.GetValue(flagClear);
                Handler(name, value, query, clear);
            });

            return root.Parse(args).Invoke();
        }

        private static void Handler(string? name, string? value, string? query, bool clear)
        {
            if (clear) Ally.ClearAliases(); // Clear
            else if (query != null) DisplayAliases(Ally.GetAliases(query)); // Search
            else if (name == null) DisplayAliases(Ally.IterAliases()); // Display
            else if (value == null) Ally.DeleteAlias(name); // Delete
            else Ally.RegisterAlias(new(name, value)); // Register
        }

        private static void DisplayAliases(IEnumerable<Alias> aliases)
        {
            foreach (Alias alias in aliases) Console.WriteLine($"ally {alias.Name} {alias.Value}");
        }
    }
}
