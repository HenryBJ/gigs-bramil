FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy necessary files and restore as distinct layer
COPY ["./Lottery.csproj","."]
RUN dotnet restore "./Lottery.csproj"  

# Copy everything else and build
COPY . ./
RUN dotnet publish "./Lottery.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env "/app/out" .

# Expose ports
EXPOSE 443/tcp


# Start
ENTRYPOINT ["dotnet", "Lottery.dll"]