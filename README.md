# Ally: CLI tool for managing unix-like aliases on windows.
## Installation
1. Download the latest release from https://github.com/priyamkalra0/Ally/releases
2. Run `ally.exe` once or create an empty directory at `%APPDATA%/Ally`
3. Move the executable `ally.exe` to `%APPDATA%/Ally`
4. Add `%APPDATA%/Ally` to PATH and restart your shell.
   
## Usage
### `ally [<name> [<value>]] [options]`  

Without arguments, `ally` prints a list of aliases in the reusable form `ally <name> <value>` on standard output.  
Otherwise, if both `<name>` & `<value>` are passed, an alias is defined binding `<name>` to `<value>`,
and if only `<name>` is passed, any existing alias corresponding to `<name>` is removed.

Options:  
  `-s, --search <query>`  Display all aliases that contain `<query>`
  `-c, --clear`           Clear all currently set aliases
  `--version`             Show version information  
  `-?, -h, --help`        Show help and usage information  

## Examples
### `ally say echo`
Now the command `say` will redirect to `echo`.  
You can test it by running `say yes`.
This should print `yes` to the console just like `echo yes` would.
### `ally alias ally` 
Now the command `alias` will redirect to `ally`.  
You can test it by running `alias -h`.   

## Features
### 1. Forwarding parameters in aliases
By default, all parameters given when calling aliases are forwarded to `<value>`.  
To disable parameter forwarding for a particular alias, append `%!` at the end of `<value>` when defining the alias.

### 2. Enable delayed expansion of environment variables
Additionally, you may use a preceding `!` to escape environment variables in aliases. Example:
```
ally show-profile "echo !%USERPROFILE!%"
```
Now, the environment variable `USERPROFILE` will be evaluated every time when the alias is called, not when it is defined.

### 3. Indepedent command chaining using special `!&!` operator
You may use `!&!` to chain multiple commands with independent parsing in a single alias, and if you don't care about independent parsing- you can just use the normal `&` operator.
Example:
```
ally greet "echo Hello !&! echo World"
```
Now, running `greet` will execute both `echo Hello` and `echo World` in sequence, but parse them independently of each other.

### 4. Support for powershell
Ally does seem to work fine on powershell, but it may vary with different configurations. tldr; ally was designed to be used in command prompt (`cmd.exe`) and it may or may not work with powershell.

## Build
Requires the .NET 10 SDK. Run the provided script:
```cmd
scripts\build
```
Produces a Native AOT binary for `win-x64` under `bin/Release/net10.0/win-x64/native/`.

## Working
Under the hood, Ally simply manages `.cmd` files for each alias you define in its `%APPDATA%/Ally` data directory, which can then be directly invoked from command prompt.
