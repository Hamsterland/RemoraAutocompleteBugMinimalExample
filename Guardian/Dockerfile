FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Guardian/Guardian.csproj", "Guardian/"]
RUN dotnet restore "Guardian/Guardian.csproj"
COPY . .
WORKDIR "/src/Guardian"
RUN dotnet build "Guardian.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Guardian.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Guardian.dll"]
