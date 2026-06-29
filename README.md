# File Transfer Service

A .NET 8 Worker Service that functions as a file transfer engine, moving files between directories following configurable rules.

## Project Overview

This application is responsible for moving files only. It does not:
- Interpret files
- Read file contents
- Import information
- Decrypt files
- Modify files

Its sole responsibility is to move files between directories following configured transfer strategies.

## Architecture

The project follows a layered architecture with clear separation of concerns:

```
FileTransferService/
├── Configuration/
├── Core/
│   ├── Models/
│   └── Interfaces/
├── Services/
├── Infrastructure/
├── Worker.cs
├── Program.cs
└── appsettings.json
```

## Technology Stack

- .NET 8
- Worker Service
- Dependency Injection
- Microsoft.Extensions.Hosting
- Serilog
- SQLite

## Principles

- SOLID principles
- Single Responsibility Principle (SRP)
- Dependency Injection
- Configuration over Code
- Thread Safety
- Low Coupling, High Cohesion

## Development

See Sprint documentation for current implementation status.
