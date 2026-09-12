using System.Text.RegularExpressions;

namespace Ally
{
    public readonly record struct Alias(string Name, string Value);

    public static partial class Ally
    {
        private static readonly string DataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Ally"
        );

        static Ally()
        {
            // Ensure that the `Ally` data directory exists.
            Directory.CreateDirectory(DataDirectory);
        }

        // Register a given `Alias` instance.
        public static void RegisterAlias(Alias alias) =>
            CreateAliasFile(
                GetAliasFilePathFromName(alias.Name),
                AliasValueToCommandList(alias.Value)
            );

        // Deletes an alias given its name.
        public static void DeleteAlias(string name) =>
            DeleteAliasFile(GetAliasFilePathFromName(name));

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

        // Returns an `Alias` instance from its name.
        public static Alias GetAlias(string name) => LoadAliasFromFile(GetAliasFilePathFromName(name));

        // Converts an `Alias` value into a Command List, which can be written to an alias (.cmd) file.
        private static string[] AliasValueToCommandList(string value)
        {
            bool explicitDisableParamFwd = value.EndsWith(" %!");
            if (explicitDisableParamFwd) value = value[..^3];

            // Using parameters manually implies no forwarding.
            bool implicitDisableParamFwd = RegexpImplicitDisableParamFwd.IsMatch(value);

            string parameterFwdSuffix = (explicitDisableParamFwd || implicitDisableParamFwd) ? "" : " %*";

            return [
                "@echo off",
                value.Replace("!%", "%") // "Unescape" environment variables
                .Replace(" !&! ", "\n").Replace("!&!", "\n") // v1.1: multiple command support
                + parameterFwdSuffix
            ];
        }

        // Loads an alias (.cmd) file into an `Alias` instance.
        private static Alias LoadAliasFromFile(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            var command_list = 
                File.ReadAllLines(path).AsSpan()
                [1..] // Discard header
                ;

            string value =
                string.Join(" !&! ", command_list) // v1.1: multiple command support
                .Replace("\"", "\\\"") // Escape quotes
                .Replace("%", "!%") // "Escape" environment variables
                ;


            bool explicitEnableParamFwd = value.EndsWith(" !%*");

            if (explicitEnableParamFwd) value = value[..^4];
            else value += " %!";

            return new(name, $"\"{value}\"");
        }

        // Alias name / file path getters.
        private static string GetAliasFilePathFromName(string name) => Path.Combine(DataDirectory, $"{name}.cmd");
        private static string GetAliasNameFromFilePath(string path) => Path.GetFileNameWithoutExtension(path);

        // Filesystem methods for alias (.cmd) files.
        private static void DeleteAliasFile(string path) => File.Delete(path);
        private static void CreateAliasFile(string path, string[] command_list) => File.WriteAllLines(path, command_list);

        // Yields names of required alias (.cmd) files, filtering as needed.
        private static IEnumerable<string> IterAliasFiles(Func<string, bool>? filter = null)
        {
            IEnumerable<string> files =
                Directory
                .EnumerateFiles(DataDirectory)
                .Where(path => path.EndsWith(".cmd"));

            if (filter == null) return files;
            return files.Where(path => filter(GetAliasNameFromFilePath(path)));
        }

        [GeneratedRegex(@"%[*,0-9]")]
        private static partial Regex RegexpImplicitDisableParamFwd { get; }
    }
}
