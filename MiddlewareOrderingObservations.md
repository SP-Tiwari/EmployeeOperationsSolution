# Middleware Ordering Observations

The final middleware order in `EmployeeOperationsMvc/Program.cs` keeps `UseStaticFiles()` before `MaintenanceModeMiddleware`:

1. Request logging
2. Execution-time measurement
3. HTTPS redirection
4. Static files
5. Routing
6. Maintenance mode
7. Conditional requests
8. Authorization
9. MVC endpoint mapping

## Temporary reorder experiment

I temporarily moved `MaintenanceModeMiddleware` before `UseStaticFiles()` and requested both a dynamic MVC route and a file under `wwwroot` while maintenance mode was enabled. Both requests received the maintenance response with HTTP 503, because maintenance middleware returned a response before static-file middleware could inspect the request.

After restoring `UseStaticFiles()` before `MaintenanceModeMiddleware`, static-file requests were served normally, while dynamic requests were still short-circuited with HTTP 503. This is the current configuration.

## Pipeline behavior

Maintenance middleware short-circuits the pipeline by writing the status code and response body and then returning without calling `_next(context)`. As a result, downstream routing, conditional middleware, authorization, and MVC endpoint execution do not run for a request rejected during maintenance.

Static files remain available because static-file middleware runs earlier in the pipeline. It can find and serve a matching file before the request reaches maintenance middleware. Requests that are not handled as static files continue to maintenance middleware and receive the maintenance response when maintenance mode is enabled.

The `/middleware-demo/forbidden` demonstration path is handled by conditional middleware when maintenance mode is disabled, or when the middleware is temporarily reordered for that experiment; it returns HTTP 403 and also short-circuits the pipeline.
