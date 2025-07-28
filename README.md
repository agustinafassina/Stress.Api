## Stress.Api
This API provides endpoints for stress testing by simulating multiple requests. Use it to evaluate the performance and stability of your system.

### Api Diagram
<img src="api-diagram.png" alt="Logo del proyecto" width="400" height="450">

### Authorization in the Api
It implements JWT authentication to secure endpoints, validating issuer, audience, and signature, allowing access only to authorized users.
```
[Authorize(AuthenticationSchemes = "Auth0App1")]
[Authorize(AuthenticationSchemes = "Auth0App2")]
```
Environment variables setting (auth0 in this case)
```
  "Auth0App1": {
    "Issuer": "https://test.asdasdasd.auth0/",
    "Audience": "Test-Api"
  },
  "Auth0App2": {
    "Issuer": "AgusFassina",
    "Audience": "Agusfassina"
  }
```

## Dotnet build and run
```
dotnet build
dotnet run
```

## Docker build and run

```
# Docker build
docker build -f Dockerfile -t api .
# Docker run in the port 8787
docker run -d -p 8787:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name api api
# api tests http://localhost:8787/swagger/index.html
```


### Endpoints and detail
1- /stress/cpu: Measure CPU usage under high load
2- /stress/memory: Controlled memory usage
3- /stress/request: Generate many requests in a short period
4- /stress/latency: Measure response latency
5- /stress/gc: Force garbage collection to evaluate memory handling
6- /stress/thread: Spawn multiple threads/tasks to test multithreaded concurrency
7- /stress/cpu-intensity: Sustained CPU load to test throttling and stability
8- /stress/load: Generate combined load (CPU, memory, requests)

