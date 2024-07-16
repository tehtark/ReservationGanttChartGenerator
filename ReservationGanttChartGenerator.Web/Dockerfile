#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Install required packages for font management
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        fontconfig \
        fonts-liberation

# Accept the Microsoft font license
RUN echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | debconf-set-selections


USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ReservationGanttChartGenerator.Web/ReservationGanttChartGenerator.Web.csproj", "ReservationGanttChartGenerator.Web/"]
COPY ["ReservationGanttChartGenerator.Application/ReservationGanttChartGenerator.Application.csproj", "ReservationGanttChartGenerator.Application/"]
COPY ["ReservationGanttChartGenerator.Domain/ReservationGanttChartGenerator.Domain.csproj", "ReservationGanttChartGenerator.Domain/"]
RUN dotnet restore "./ReservationGanttChartGenerator.Web/ReservationGanttChartGenerator.Web.csproj"
COPY . .
WORKDIR "/src/ReservationGanttChartGenerator.Web"


FROM build AS publish
WORKDIR /app
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ReservationGanttChartGenerator.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
COPY --from=publish /app/publish .

RUN dotnet build "./ReservationGanttChartGenerator.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

ENTRYPOINT ["dotnet", "ReservationGanttChartGenerator.Web.dll"]