
#need combile 1st.
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS runtime
WORKDIR /app
COPY / /app/
ENTRYPOINT ["dotnet", "NSmartProxyClient.dll"]