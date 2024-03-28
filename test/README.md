# Run tests
## Basic
`dotnet test`
## With Filter (UnitTests only)
`dotnet test .\TechStack.sln --filter Category=UnitTest`
## With Code Coverage
`dotnet test .\TechStack.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`
`dotnet test .\TechStack.sln /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura`
* output format "cobertura" can also be read from VS Code Coverage Gutters extension