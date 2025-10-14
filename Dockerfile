FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/projApiMongoDB.Api/projApiMongoDB.Api.csproj", "src/projApiMongoDB.Api/"]
RUN dotnet restore "src/projApiMongoDB.Api/projApiMongoDB.Api.csproj"
COPY . .
WORKDIR "/src/src/projApiMongoDB.Api"
RUN dotnet publish "projApiMongoDB.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "projApiMongoDB.Api.dll"]
