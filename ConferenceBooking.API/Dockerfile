# Багатоетапна збірка: у фінальний образ потрапляє тільки runtime і опубліковані
# файли, без SDK та вихідного коду. Образ менший, поверхня атаки вужча.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Спочатку копіюємо лише .csproj і відновлюємо пакети окремим шаром:
# поки залежності не змінилися, Docker бере цей крок з кешу.
COPY ["ConferenceBooking.API/ConferenceBooking.API.csproj", "ConferenceBooking.API/"]
COPY ["ConferenceBooking.Application/ConferenceBooking.Application.csproj", "ConferenceBooking.Application/"]
COPY ["ConferenceBooking.Domain/ConferenceBooking.Domain.csproj", "ConferenceBooking.Domain/"]
COPY ["ConferenceBooking.Infrastructure/ConferenceBooking.Infrastructure.csproj", "ConferenceBooking.Infrastructure/"]
RUN dotnet restore "ConferenceBooking.API/ConferenceBooking.API.csproj"

COPY . .
RUN dotnet publish "ConferenceBooking.API/ConferenceBooking.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Образ chiseled уже містить непривілейованого користувача app (UID 1654),
# тому процес не працює від root.
USER app

COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ConferenceBooking.API.dll"]