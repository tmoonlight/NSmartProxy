﻿
#need combile 1st.
FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY / /app/
ENTRYPOINT ["dotnet", "NSmartProxyClient.dll"]