FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TraceFlow.LogProcessor/TraceFlow.LogProcessor.csproj TraceFlow.LogProcessor/
RUN dotnet restore TraceFlow.LogProcessor/TraceFlow.LogProcessor.csproj

COPY TraceFlow.LogProcessor/ TraceFlow.LogProcessor/
RUN dotnet publish TraceFlow.LogProcessor/TraceFlow.LogProcessor.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TraceFlow.LogProcessor.dll"]