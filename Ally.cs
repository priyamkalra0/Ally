using System.Text.RegularExpressions;

namespace Ally
{
    public readonly struct Alias
    {
        public readonly string Name { get; }
        public readonly string Value { get; }

        public Alias(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public static class Ally
    {
        private static readonly string DataDirectory = Environment.GetEnvironmentVariable("APPDATA") + @"\Ally";

        static Ally()
        {
            // Ensure that the `Ally` data directory exists.
            Directory.CreateDirectory(DataDirectory);
        }

        // Register a given `Alias` instance.
        public static void RegisterAlias(Alias alias) =>
            CreateAliasFile(
                GetAliasFilePath(alias.Name),
                DumpAliasIntoFileBuilder(alias.Value)
            );

        // Deletes an alias given it's name.
        public static void DeleteAlias(string name) =>
            DeleteAliasFile(GetAliasFilePath(name));

        // Clears all registered aliases.
        public static void ClearAliases()
        {
            foreach (Alias alias in IterAliases())
                DeleteAlias(alias.Name);
        }

        // Yields `Alias` instances which contain given query in their names.
        public static IEnumerable<Alias> GetAliases(string query) => 
            IterAliases(name => name.Contains(query));

        // Yields required `Alias` instances, filtering as needed.
        public static IEnumerable<Alias> IterAliases(Func<string, bool>? filter = null)
        {
            return
                from file in IterAliasFiles(filter)
                select LoadAliasFromFile(file);
        }

        // Returns an `Alias` instance from it's name.
        public static Alias GetAlias(string name) => LoadAliasFromFile(GetAliasFilePath(name));

        // Prepares the contents required to build an alias (.cmd) file for a given `Alias` instance.
        private static string[] DumpAliasIntoFileBuilder(string value)
        {
            bool explicitDisableParamFwd = value.EndsWith(" %!");
            if (explicitDisableParamFwd) value = value[..^3];

            // Using parameters manually implies no forwarding.
            bool implicitDisableParamFwd = new Regex(@"\%[*,0-9]").IsMatch(value);

            string parameterFwdSuffix = (explicitDisableParamFwd || implicitDisableParamFwd) ? "" : " %*";

            return new[] {
                "@echo off",
                value.Replace("!%", "%") // "Unescape" enviroment variables
                + parameterFwdSuffix
            };
        }

        // Essentially reverses the dumping process, converting an alias (.cmd) file into an `Alias`.
        private static Alias LoadAliasFromFile(string path)
        {
            string name = new FileInfo(path).Name[..^4];
            string value = 
                File.ReadAllLines(path)[^1] // Discard header
                .Replace("\"", "\\\"") // Escape quotes
                .Replace("%", "!%"); // "Escape" enviroment variables


            bool explicitEnableParamFwd = value.EndsWith(" !%*");

            if (explicitEnableParamFwd) value = value[..^4];
            else value += " %!";

            return new(name, $"\"{value}\"");
        }

        // Simply returns the assumed path of an alias (.cmd) file.
        private static string GetAliasFilePath(string name) => (DataDirectory + @$"\{name}.cmd");

        // FS Wrappers for alias (.cmd) files.
        private static void DeleteAliasFile(string path) => File.Delete(path);
        private static void CreateAliasFile(string path, string[] builder) => File.WriteAllLines(path, builder);

        // Yields names of required alias (.cmd) files, filtering as needed.
        private static IEnumerable<string> IterAliasFiles(Func<string, bool>? filter = null)
        {
            IEnumerable<string> files =
                Directory
                .EnumerateFiles(DataDirectory)
                .Where(name => name.EndsWith(".cmd"));

             if (filter == null) return files;
            return files.Where(filter);
        }
    }
}
