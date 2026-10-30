FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TraceFlow.LogProcessor/TraceFlow.LogProcessor.csproj TraceFlow.LogProcessor/
RUN dotnet restore TraceFlow.LogProcessor/TraceFlow.LogProcessor.csproj

COPY TraceFlow.LogProcessor/ TraceFlow.LogProcessor/

RUN dotnet publish TraceFlow.LogProcessor/TraceFlow.LogProcessor.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl unzip procps \
    && rm -rf /var/lib/apt/lists/*

RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TraceFlow.LogProcessor.dll"]