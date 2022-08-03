#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./.", "Performance_Producer"]
#COPY ["Performance_Producer/Performance_Producer.csproj", "Performance_Producer/"]
RUN ls Performance_Producer

#RUN dotnet restore "Performance_Producer/Performance_Producer.csproj"
#COPY . .
#WORKDIR "/src/Performance_Producer"
#RUN dotnet build "Performance_Producer.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "Performance_Producer.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Performance_Producer.dll"]