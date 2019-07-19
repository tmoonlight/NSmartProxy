
#need combile 1st.
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime
WORKDIR /app
COPY / /app/
ENTRYPOINT ["dotnet", "NSmartProxyClient.dll"]