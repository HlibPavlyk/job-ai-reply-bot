FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DjinniAIReplyBot.Api/DjinniAIReplyBot.Api.csproj", "src/DjinniAIReplyBot.Api/"]
COPY ["src/DjinniAIReplyBot.Infrastructure/DjinniAIReplyBot.Infrastructure.csproj", "src/DjinniAIReplyBot.Infrastructure/"]
COPY ["src/DjinniAIReplyBot.Application/DjinniAIReplyBot.Application.csproj", "src/DjinniAIReplyBot.Application/"]
COPY ["src/DjinniAIReplyBot.Domain/DjinniAIReplyBot.Domain.csproj", "src/DjinniAIReplyBot.Domain/"]
RUN dotnet restore "src/DjinniAIReplyBot.Api/DjinniAIReplyBot.Api.csproj"
COPY . .
WORKDIR "/src/src/DjinniAIReplyBot.Api"
RUN dotnet build "DjinniAIReplyBot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DjinniAIReplyBot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DjinniAIReplyBot.Api.dll"]
