FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MiApiParcial/MiApiParcial.csproj", "MiApiParcial/"]
RUN dotnet restore "MiApiParcial/MiApiParcial.csproj"
COPY . .
WORKDIR "/src/MiApiParcial"
RUN dotnet build "MiApiParcial.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiApiParcial.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiApiParcial.dll"]
