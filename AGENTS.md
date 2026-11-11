# AGENTS.md

## Git

- Never commit changes unless the user explicitly asks you to.

## C# style

- Use modern C# syntax (the API targets .NET 10 with `LangVersion` latest) and follow the IDE's style suggestions.

## Running the apps

- The API runs on http://localhost:5033 and the web app on http://localhost:3000. See the README for the start commands.
- When you start either app to check something, run it as a process you can stop again (a tracked background task, not a bare `&` in a shell command), and stop it before you finish. Check that ports 5033 and 3000 are free before starting, so you don't run on top of an app the user already has running.
