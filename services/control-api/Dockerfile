FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TraceFlow.api/TraceFlow.api.csproj TraceFlow.api/
RUN dotnet restore TraceFlow.api/TraceFlow.api.csproj

COPY TraceFlow.api/ TraceFlow.api/
RUN dotnet publish TraceFlow.api/TraceFlow.api.csproj \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TraceFlow.Api.dll"]