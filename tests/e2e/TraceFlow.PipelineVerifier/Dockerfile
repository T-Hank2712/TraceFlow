FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY TraceFlow.PipelineVerifier/TraceFlow.PipelineVerifier.csproj TraceFlow.PipelineVerifier/
RUN dotnet restore TraceFlow.PipelineVerifier/TraceFlow.PipelineVerifier.csproj

COPY TraceFlow.PipelineVerifier/ TraceFlow.PipelineVerifier/
RUN dotnet publish TraceFlow.PipelineVerifier/TraceFlow.PipelineVerifier.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TraceFlow.PipelineVerifier.dll"]
