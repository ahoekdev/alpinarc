# CLAUDE.md

## Running the apps

- The API runs on http://localhost:5033 and the web app on http://localhost:3000. See the README for the start commands.
- When you start either app to check something, start it as a background task, never with `&` inside a shell command, and stop it before you finish. Check that ports 5033 and 3000 are free before starting, so you don't run on top of an app the user already has running.
