FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RVM.ShopEngine.API/RVM.ShopEngine.API.csproj", "src/RVM.ShopEngine.API/"]
COPY ["src/RVM.ShopEngine.Domain/RVM.ShopEngine.Domain.csproj", "src/RVM.ShopEngine.Domain/"]
COPY ["src/RVM.ShopEngine.Infrastructure/RVM.ShopEngine.Infrastructure.csproj", "src/RVM.ShopEngine.Infrastructure/"]
RUN dotnet restore "./src/RVM.ShopEngine.API/RVM.ShopEngine.API.csproj"
COPY . .
WORKDIR "/src/src/RVM.ShopEngine.API"
RUN dotnet build "./RVM.ShopEngine.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RVM.ShopEngine.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RVM.ShopEngine.API.dll"]
