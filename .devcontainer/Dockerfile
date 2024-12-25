# Use the official .NET 9 SDK image as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base

# Install MongoDB tools
RUN apt-get update && apt-get install -y \
    gnupg \
    wget && \
    wget -qO - https://www.mongodb.org/static/pgp/server-4.4.asc | apt-key add - && \
    echo "deb [ arch=amd64 ] https://repo.mongodb.org/apt/debian buster/mongodb-org/4.4 main" | tee /etc/apt/sources.list.d/mongodb-org-4.4.list && \
    apt-get update && apt-get install -y \
    mongodb-org-tools && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /workspace

# Expose the port MongoDB will use
EXPOSE 27017

# Run the default command
CMD ["dotnet", "--version"]
