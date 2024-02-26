# UserManager

## Introduction

This tool uses the `MessageReader` component to parse commands from the console for managing user data. This program was
written as a test task.

## Key Features

- **MessageReader**: This component handles reading console input, breaking it down into manageable commands.
- **CommandStreamProcessor**: Parses messages into commands.
- **Database Integration**: Entity Framework Core and SQLite database.
- **Ardalis.Specification**

## Setup

Before you start, make sure you've installed all necessary NuGet packages. Set the `SQLITE_CONNECTION` environment
variable.

## Usage Instructions

Run the application and enter commands in JSON format, one per line. Available commands include fetching users by ID and
domain, listing users by domain, and finding users by tag. Here's an example:

```json
{ "CommandType": "get_user", "Domain": "Technology", "Tag": "Music" }
```

## Important Notes

- Ensure your JSON commands are correctly formatted for the application to understand them.
- Json must me single-line. The '\n' character means the end of command.
- File 'test' contains test commands. You can simple 'Ctrl-A Ctrl-C Ctrl-V' them into the console.

## Dependencies

- .NET Core 8.0
- EF Core
- Ardalis.Specification