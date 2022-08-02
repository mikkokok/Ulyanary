#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0-nanoserver-1809 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-nanoserver-1809 AS build
WORKDIR /src
COPY ["Ulyanary/Ulyanary.csproj", "Ulyanary/"]
RUN dotnet restore "Ulyanary/Ulyanary.csproj"
COPY . .
WORKDIR "/src/Ulyanary"
RUN dotnet build "Ulyanary.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ulyanary.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ulyanary.dll"]