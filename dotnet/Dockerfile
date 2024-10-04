# Use the official ASP.NET Core image as the base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 443

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["dotnet.csproj", "./"]
RUN dotnet restore "./dotnet.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "dotnet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet.csproj" -c Release -o /app/publish

# Build the final image using the base image and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dotnet.dll"]
