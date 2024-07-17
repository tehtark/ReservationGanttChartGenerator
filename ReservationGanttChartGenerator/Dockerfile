FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install required packages for font management
RUN echo "deb http://deb.debian.org/debian/ bookworm main contrib" > /etc/apt/sources.list && \
    echo "deb-src http://deb.debian.org/debian/ bookworm main contrib" >> /etc/apt/sources.list && \
    echo "deb http://security.debian.org/ bookworm-security main contrib" >> /etc/apt/sources.list && \
    echo "deb-src http://security.debian.org/ bookworm-security main contrib" >> /etc/apt/sources.list
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update && apt-get install -y ttf-mscorefonts-installer fontconfig

USER app
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ReservationGanttChartGenerator/ReservationGanttChartGenerator.csproj", "ReservationGanttChartGenerator/"]
COPY ["ReservationGanttChartGenerator.Application/ReservationGanttChartGenerator.Application.csproj", "ReservationGanttChartGenerator.Application/"]
COPY ["ReservationGanttChartGenerator.Domain/ReservationGanttChartGenerator.Domain.csproj", "ReservationGanttChartGenerator.Domain/"]
RUN dotnet restore "./ReservationGanttChartGenerator/ReservationGanttChartGenerator.csproj"
COPY . .
WORKDIR "/src/ReservationGanttChartGenerator"
RUN dotnet build "./ReservationGanttChartGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ReservationGanttChartGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReservationGanttChartGenerator.dll"]