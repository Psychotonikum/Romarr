# Contributing

## How to Contribute

Contributions are welcome. Here's how you can help.

### Bug Reports

1. Search [existing issues](https://github.com/Psychotonikum/Romarr/issues) to avoid duplicates
2. Open a new issue with:
   - Clear description of the bug
   - Steps to reproduce
   - Expected vs actual behavior
   - Romarr version, OS, and runtime version
   - Relevant log excerpts (set log level to Debug)

### Feature Requests

Open an issue with the "feature request" label. Describe:
- The problem it solves
- How you envision it working
- Why it fits the Romarr project

### Code Contributions

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes
4. Run tests: `dotnet test src/Romarr.Core.Test/Romarr.Core.Test.csproj`
5. Build both backend and frontend to verify:
   ```bash
   dotnet build src/Romarr.sln
   yarn build
   ```
6. Commit with a clear message
7. Push and open a Pull Request

### Documentation

Improvements to README, docs, and wiki are always appreciated. Just edit and submit a PR.

## Development Setup

See [docs/development.md](../docs/development.md) for full development environment setup.

### Quick Start

```bash
git clone https://github.com/Psychotonikum/Romarr.git
cd romarr
dotnet restore src/Romarr.sln
dotnet build src/Romarr.sln
yarn install
yarn start  # Frontend dev server with hot reload
```

## Code Guidelines

- Follow existing code style (StyleCop rules in `.editorconfig`)
- Use `var` for variable declarations in C#
- 4-space indentation
- Write tests for new functionality
- Keep commits focused and atomic

## License

By contributing, you agree that your contributions will be licensed under the GNU GPL v3.
