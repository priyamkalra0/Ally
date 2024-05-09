# Ally: CLI tool for managing unix-like aliases on windows.
## Installation
1. Download the latest release from https://github.com/priyamkalra0/Ally/releases
2. Run ally once or create an empty directory at `%APPDATA%/Ally`
3. Move the executable `ally.exe` to `%APPDATA%/Ally`
4. Add `%APPDATA%/Ally` to PATH and restart your shell.
   
## Usage
### `ally [<name> [<value>]] [options]`  

Without arguments, `ally` prints a list of aliases in the reusable form `ally <name> <value>` on standard output.  
Otherwise, if both `<name>` & `<value>` are passed, an alias `<name>` is defined for some `<value>`,
and if only `<name>` is passed, any existing alias corresponding to `<name>` is removed.

Options:  
  `-s, --search <query>`  Display all aliases that contain `<query>`  
  `-c, --clear`           Clear all currently set aliases.  
  `--version`             Show version information  
  `-?, -h, --help`        Show help and usage information  

## Examples
### `ally say echo`
Now the command `say` will be equivalent to `echo`.  
You can test it by running `say yes`.
This should print yes to the console just like `echo yes` would.
### `ally alias ally` 
Now the command `alias` will be equivalent to `ally`.  
You can test it by running `alias -h`.   


## Notes
### 1. Not tested on powershell
I have not extensively tested ally on powershell. On my current machine, it does seem to work fine, but it may vary with different configurations. tldr; ally is not meant to be used with powershell and it may or may not work.

### 2. Forwarding parameters to aliases
By default, all parameters given when calling aliases are forwaded to `<value>`.  
To disable parameter forwarding for a particular alias, append `%!` at the end of `<value>` when defining the alias.

### 3. Delayed expansion of enviroment variables
Additionally, you may use a preceding `!` to escape enviroment variables in aliases.

Ex. `ally show-profile "echo !%USERPROFILE!%"`

Now, the enviroment variable will be expanded each time the alias is called, not when it is defined.

## Working
Under the hood, Ally simply manages `.cmd` files for each alias you define in it's `%APPDATA%/Ally` data directory, which can then be directly invoked from command prompt.
