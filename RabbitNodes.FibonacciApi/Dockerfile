FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitNodes.FibonacciApi/RabbitNodes.FibonacciApi.csproj", "RabbitNodes.FibonacciApi/"]
COPY ["RabbitNodes.Shared/RabbitNodes.Shared.csproj", "RabbitNodes.Shared/"]
COPY ["TrueCodeTest.RpcClient/TrueCodeTest.RpcClient.csproj", "TrueCodeTest.RpcClient/"]
RUN dotnet restore "RabbitNodes.FibonacciApi/RabbitNodes.FibonacciApi.csproj"
COPY . .
WORKDIR "/src/RabbitNodes.FibonacciApi"
RUN dotnet build "RabbitNodes.FibonacciApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RabbitNodes.FibonacciApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitNodes.FibonacciApi.dll"]
