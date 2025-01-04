# Use the official .NET runtime image for the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Use the SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Project_A_Server.csproj", "./"]
RUN dotnet restore "./Project_A_Server.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet publish "./Project_A_Server.csproj" -c Release -o /app/publish

# Use the runtime image for the final output
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Project_A_Server.dll"]
