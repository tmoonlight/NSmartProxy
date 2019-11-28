#FROM microsoft/dotnet:2.2-sdk AS build
#WORKDIR /app
#
## copy csproj and restore as distinct layers
#COPY *.csproj ./
#RUN dotnet restore
#
## copy everything else and build app
#COPY . ./
#RUN dotnet publish -c Release -o out
#
#FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime
#WORKDIR /app
#COPY --from=build /app/out ./
#EXPOSE 12300-22300
#ENTRYPOINT ["dotnet", "NSmartProxy.ServerHost.dll"]

#need combile 1st.
FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY / /app/
EXPOSE 12300-22300
ENTRYPOINT ["dotnet", "NSmartProxy.ServerHost.dll"]