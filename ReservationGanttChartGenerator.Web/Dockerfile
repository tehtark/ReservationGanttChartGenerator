FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install required packages for font management
RUN echo "deb http://deb.debian.org/debian/ bookworm main contrib" > /etc/apt/sources.list && \
    echo "deb-src http://deb.debian.org/debian/ bookworm main contrib" >> /etc/apt/sources.list && \
    echo "deb http://security.debian.org/ bookworm-security main contrib" >> /etc/apt/sources.list && \
    echo "deb-src http://security.debian.org/ bookworm-security main contrib" >> /etc/apt/sources.list
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update && apt-get install -y ttf-mscorefonts-installer fontconfig

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the csproj files first for better caching
COPY ReservationGanttChartGenerator.Web/ReservationGanttChartGenerator.Web.csproj ./ReservationGanttChartGenerator.Web/
COPY ReservationGanttChartGenerator.Application/ReservationGanttChartGenerator.Application.csproj ./ReservationGanttChartGenerator.Application/
COPY ReservationGanttChartGenerator.Domain/ReservationGanttChartGenerator.Domain.csproj ./ReservationGanttChartGenerator.Domain/

# Restore dependencies
RUN dotnet restore ReservationGanttChartGenerator.Web/ReservationGanttChartGenerator.Web.csproj

# Copy the rest of the source code
COPY . .

# Publish the project (note the changed WORKDIR)
WORKDIR /src/ReservationGanttChartGenerator.Web
RUN dotnet publish "ReservationGanttChartGenerator.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ReservationGanttChartGenerator.Web.dll"]