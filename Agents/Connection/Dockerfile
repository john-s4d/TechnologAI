FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Agents/Connection/Connection.csproj", "Agents/Connection/"]
RUN dotnet restore "Agents/Connection/Connection.csproj"
COPY . .
WORKDIR "/src/Agents/Connection"
RUN dotnet build "Connection.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Connection.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Connection.dll"]
