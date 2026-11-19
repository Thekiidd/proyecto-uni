# Token Economy Rules

- **Surgical File Reads**: Always pass `StartLine` and `EndLine` (prefer <= 150 lines) to `read_file`. Never re-read unchanged slices already in context.
- **No Raw Unity YAML Reads**: Never dump `.unity`, `.prefab`, or `.asset` YAML files into context; query targeted fields via Python/PowerShell or C# scripts.
- **Compact Terminal Output**: Route supported CLI commands (`git status`, `git diff`, `git log`, `npm`, `cargo`, `pytest`) through `C:\Users\lbati\.local\bin\rtk.exe`, and cap custom PowerShell/compiler output (e.g., `Select-Object -First 35`, `/nologo`, quiet flags).
- **Large Data in Sandbox**: Use `context-mode` MCP tools (when active) or local scratch scripts to filter large logs/JSONs locally, returning only final summaries.
- **Lean Subagents & Replies**: Use `Model: "flash_lite"` or `"flash"` for read-only `research` subagents when broad exploration is needed. Keep chat replies concise and skip artifacts for short answers.
