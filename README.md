# Market Assets API (Magnise Test Task)

This is a .NET 8 REST API built to fetch and serve market asset prices. It uses the Fintacharts platform under the hood, pulling historical data via their REST API and listening for real-time price updates through WebSockets.

The project is structured using Clean Architecture principles (Api, Application, Domain, Infrastructure). For the database, I went with SQLite to make it easy to review and run. You don't need to set up any external SQL servers or run manual migrations — the database is automatically created and seeded with supported assets from Fintacharts the very first time you start the app.

Real-time prices are handled by a background hosted service. It authenticates with Fintacharts, opens a WebSocket connection, and safely caches the latest prices in memory so the API can return them instantly without waiting.

### How to run

The entire solution is dockerized. To start it, just open your terminal in the root folder (where the `docker-compose.yml` is located) and run:

```bash
docker-compose up --build

Once the containers are up and the initial data is seeded, the API will be listening on port 8080.

Endpoints to test
Here are the main endpoints you can hit via browser or Postman:

1. Get all supported assets
GET http://localhost:8080/api/assets

2. Get historical prices
GET http://localhost:8080/api/prices/history?symbol=EURUSD&from=2024-03-10&to=2024-03-20

3. Get the latest real-time price
GET http://localhost:8080/api/prices/realtime?symbol=EURUSD
