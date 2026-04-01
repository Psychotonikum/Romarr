# Contributing to Romarr

Thank you for your interest in contributing to Romarr.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.103+)
- [Node.js 20+](https://nodejs.org/)
- [Yarn](https://yarnpkg.com/) (v1.x)
- Git

## Development Setup

```bash
git clone https://github.com/Psychotonikum/Romarr.git
cd romarr

# Automated setup for Debian/Ubuntu:
sudo bash scripts/setup-dev.sh

# Or manually:
yarn install && yarn build
dotnet msbuild -restore src/Romarr.sln -p:Configuration=Debug -p:Platform=Posix
```

## Running

```bash
./_output/net10.0/Romarr
# Web UI: http://localhost:9797
```

For frontend hot reload during development:

```bash
yarn start
```

## Testing

```bash
# Run unit tests (excludes integration tests)
dotnet test src/Romarr.sln --filter 'Category!=IntegrationTest&Category!=AutomationTest&Category!=ManualTest'
```

## Pull Request Process

1. Fork the repository
2. Create a feature branch from `main`
3. Make your changes
4. Run the test suite
5. Submit a pull request to `main`

## Code Style

- Follow existing code conventions in the project
- C# code should follow the .editorconfig and stylecop rules
- Frontend code should pass `yarn lint` and `yarn stylelint`

## Architecture Notes

Romarr is a fork of [Sonarr](https://github.com/Sonarr/Sonarr). The domain model maps TV concepts to gaming:

| Sonarr | Romarr |
|--------|---------|
| Series | Game |
| Season | Platform |
| Episode | ROM |

See [docs/development.md](docs/development.md) for the full architecture guide.

## License

By contributing, you agree that your contributions will be licensed under the [GNU GPL v3](LICENSE.md).
