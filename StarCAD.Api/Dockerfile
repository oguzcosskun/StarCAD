# Build context is the REPO ROOT (see docker-compose.yml): the API references
# six sibling projects, so a context rooted at StarCAD.Api/ could not reach them.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# csproj files first, restore, then the rest: source edits do not invalidate the
# restore layer, so a rebuild after a code change skips the NuGet download.
COPY StarCAD.Api/StarCAD.Api.csproj StarCAD.Api/
COPY StarCAD.Business/StarCAD.Business.csproj StarCAD.Business/
COPY StarCAD.Common/StarCAD.Common.csproj StarCAD.Common/
COPY StarCAD.Core/StarCAD.Core.csproj StarCAD.Core/
COPY StarCAD.DataAccess/StarCAD.DataAccess.csproj StarCAD.DataAccess/
COPY StarCAD.Entity/StarCAD.Entity.csproj StarCAD.Entity/
COPY StarCAD.ViewModel/StarCAD.ViewModel.csproj StarCAD.ViewModel/
RUN dotnet restore StarCAD.Api/StarCAD.Api.csproj

COPY . .
RUN dotnet publish StarCAD.Api/StarCAD.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# HTTP only. TLS terminates at the reverse proxy in production, and a dev
# certificate inside the container would just break browser fetches.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StarCAD.Api.dll"]
