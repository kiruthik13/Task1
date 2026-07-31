# Build stage using .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HospitalManagement.Web.csproj", "./"]
RUN dotnet restore "HospitalManagement.Web.csproj"
COPY . .
RUN dotnet publish "HospitalManagement.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage using ASP.NET Core 10 Runtime with Kerberos libgssapi dependency for Npgsql
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && \
    apt-get install -y libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HospitalManagement.Web.dll"]
