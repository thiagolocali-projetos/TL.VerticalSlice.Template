FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/TL.VerticalSlice.Template.API/TL.VerticalSlice.Template.API.csproj"
RUN dotnet build "src/TL.VerticalSlice.Template.API/TL.VerticalSlice.Template.API.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "TL.VerticalSlice.Template.API.dll"]