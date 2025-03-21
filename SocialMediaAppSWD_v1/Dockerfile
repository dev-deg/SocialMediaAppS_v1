FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SocialMediaAppSWD_v1/SocialMediaAppSWD_v1.csproj", "SocialMediaAppSWD_v1/"]
RUN dotnet restore "SocialMediaAppSWD_v1/SocialMediaAppSWD_v1.csproj"

# Copy the rest of the files
COPY . .
WORKDIR "/src/SocialMediaAppSWD_v1"

# Build the application
RUN dotnet build "SocialMediaAppSWD_v1.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SocialMediaAppSWD_v1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Install the HTTPS development certificate utilities
RUN apt-get update && \
    apt-get install -y --no-install-recommends libc6-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Create directory for Google credentials
RUN mkdir -p /app/creds

# Copy Google credentials file
COPY pftc-2025-s-d1cf2c36c73d.json /app/creds/

# Set environment variables
ENV ASPNETCORE_URLS=https://+:5001;http://+:5085
ENV ASPNETCORE_ENVIRONMENT=Development
ENV GOOGLE_APPLICATION_CREDENTIALS=/app/creds/pftc-2025-s-d1cf2c36c73d.json

# Generate development certificate for HTTPS
RUN dotnet dev-certs https --trust

# Expose the ports
EXPOSE 5001 5085

ENTRYPOINT ["dotnet", "SocialMediaAppSWD_v1.dll"]
