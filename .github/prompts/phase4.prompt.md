# Phase 4: Performance Optimization Plan for InventoryHub

## Step 1: Optimize the Application for Performance

### 1. Identify and Reduce Redundant API Calls in the Front-End
- Audit all Blazor components (especially `FetchProducts.razor`) for repeated or unnecessary API calls.
- Ensure product data is fetched only once per user session or page load, not on every render or event.
- Use state management (e.g., cascading parameters, singleton services, or local storage) to cache product data on the client.
- Refactor any logic that causes multiple fetches (e.g., avoid calling the API in both `OnInitializedAsync` and event handlers unless needed).

### 2. Implement Caching Strategies in the Back-End
- Add in-memory caching (e.g., using `IMemoryCache`) to cache product data in the API.
- Cache the `/api/products` response for 5 minutes.
- Ensure cache is invalidated or refreshed when product data changes (if applicable).
- Refactor the API endpoint to check the cache before querying or constructing the product list.

### 3. Refactor Repetitive or Inefficient Code in Both Projects
- Review both front-end and back-end code for repeated logic, unnecessary loops, or inefficient data structures.
- Consolidate duplicate code into reusable methods or services.
- Optimize LINQ queries, loops, and data transformations for performance.
- Remove any unnecessary serialization/deserialization steps.
- Ensure proper use of async/await to avoid blocking calls.

### 4. Test and Validate Performance Improvements
- Use browser dev tools and .NET logging to verify reduced API calls and faster response times.
- Confirm that caching works (API returns cached data, cache expires as expected).
- Ensure no loss of functionality or data freshness.

### 5. Implement Logging to Monitor API Response Times
- Add logging in the back-end to record the duration of each API request.
- In `ServerApp/Program.cs`, use a stopwatch (e.g., `System.Diagnostics.Stopwatch`) at the start and end of the `/api/products` endpoint.
- Log the elapsed time using the built-in logging framework (e.g., `ILogger`).
- Example:
  ```csharp
  app.MapGet("/api/products", (ILogger<Program> logger) =>
  {
      var sw = Stopwatch.StartNew();
      // ...existing code to get products...
      sw.Stop();
      logger.LogInformation($"/api/products responded in {sw.ElapsedMilliseconds} ms");
      return products;
  });
  ```
- Review logs to monitor and analyze API response times during development and testing.
