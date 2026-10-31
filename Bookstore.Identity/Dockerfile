
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Bookstore.Identity/Bookstore.Identity.csproj", "Bookstore.Identity/"]
RUN dotnet restore "Bookstore.Identity/Bookstore.Identity.csproj"

COPY . .
WORKDIR /src/Bookstore.Identity
RUN dotnet publish "Bookstore.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Bookstore.Identity.dll"]
