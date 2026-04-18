# build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
# сначала только csproj — лучше кэшируется (все зависимые проекты тоже)
COPY ./PromoCodeFactory.Core/PromoCodeFactory.Core.csproj ./PromoCodeFactory.Core/
COPY ./PromoCodeFactory.DataAccess/PromoCodeFactory.DataAccess.csproj ./PromoCodeFactory.DataAccess/
COPY ./PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj ./PromoCodeFactory.WebHost/

RUN dotnet restore ./PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj

# теперь весь код
COPY ./PromoCodeFactory.Core/ ./PromoCodeFactory.Core/
COPY ./PromoCodeFactory.DataAccess/ ./PromoCodeFactory.DataAccess/
COPY ./PromoCodeFactory.WebHost/ ./PromoCodeFactory.WebHost/

RUN dotnet publish ./PromoCodeFactory.WebHost -c Release -o /app/publish

# run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ARG API_PORT_INT
EXPOSE ${API_PORT_INT}
ENTRYPOINT ["dotnet","PromoCodeFactory.WebHost.dll"]
