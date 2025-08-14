 # App Coding Standards

 ---

 ## Logging

 All log messages are automatically prefixed by the `Log` class with the `ClassName.MethodName:` of the calling method.

 **Example:**

 A log message from a method named `GenerateScenario` within a class named `ScenarioGenerator` would be formatted as:

 `ScenarioGenerator.GenerateScenario: Scenario generation started.`

 ---

 ## UI Progress Messages

 All messages reported through `IProgress<string>` should be concise and prefixed with a severity string (`INFO:`, `WARNING:`, `ERROR:`). The purpose is to provide immediate, user-facing feedback, not to serve as a detailed log record.

 ### Usage Guidelines for Severity Prefixes:

 * **ERROR:** Use for critical failures that prevent a task from completing. These messages indicate a problem that requires user intervention or a bug.
 * **WARNING:** Use for non-critical issues or potential problems that the user should be aware of but do not halt the application's process.
 * **NOTICE:** Use for important, but not critical, notifications that draw the user's attention to a specific condition, such as an outdated cache file.
 * **INFO:** Use for routine status updates, progress reports, or general information about a task's progress.

### Message Consistency
When a log message and a UI progress message convey the same information, use a single string variable to define the message. This ensures the content is identical and simplifies future updates.

 **Example:**

 ```csharp
 string message = "Loading runways from binary cache...";
_log.InfoAsync(message);
progressReporter.Report($"INFO: {message}");
 ```

 ---

 ## XML Comments

 All **public and internal** methods, properties, and classes must include XML documentation comments. For **private** members, XML comments are encouraged for complex logic, but simple, self-explanatory private methods may use inline comments if clarity is maintained.

 * `<summary>`: A brief summary of the member's purpose.
 * `<param>`: (For methods) A description for each parameter.
 * `<returns>`: (For methods) A description of the value the method returns. When referencing language keywords like `true` or `false`, use the `<see langword=""/>` tag.

 **Example:**

 ```csharp
 /// <summary>
 /// Generates a new scenario based on user-defined parameters.
 /// </summary>
 /// <param name="scenarioType">The type of scenario to generate.</param>
 /// <returns><see langword="true"/> if the scenario was generated successfully; otherwise, <see langword="false"/>.</returns>
 private bool GenerateScenario(ScenarioType scenarioType)
 {
     // ...
 }
 ```

 ---

 ## Documentation Style: XML vs. Inline Comments

 * **XML comments** (specifically the `<summary>` and `<remarks>`) should be the primary source of documentation. Use them to explain the **high-level logic** and the **overall purpose** of a method.
 * **Inline comments** (`//`) should be used sparingly. Reserve them for explaining **complex algorithms**, **non-obvious implementation details**, or **workarounds** that are not clear from the code itself.
 * Avoid redundant inline comments that simply restate what the code or the XML summary already explains. This keeps the code clean and prevents documentation from becoming inconsistent.

 ---

 ## Naming Conventions

 * **Private Fields**: Private fields must be prefixed with a single underscore (`_`).
 * **Parameters**: Parameters and local variables must use `camelCase`.
 * **Asynchronous Methods**: All asynchronous methods (those returning `Task` or `Task<T>`) must have the `Async` suffix.

 **Example:**

 ```csharp
 // The field is prefixed with _, the parameter is not.
 private readonly ToolStripStatusLabel _statusLabel = statusLabel;

 // Asynchronous method name
 public async Task LoadDataAsync() { /* ... */ }
 ```

 ---

 ## Parameter Validation

 All public methods and constructors must use guard clauses to validate non-nullable parameters. This is not required for private members, as it is assumed that validation has been performed at the public-facing boundary. For C# 12 primary constructors, use the null-coalescing assignment operator.

 **Example (Primary Constructor):**

 ```csharp
 public class MyClass(string requiredParameter)
 {
     // Guard clause to validate a parameter in a primary constructor
     private readonly string _requiredParameter = requiredParameter ?? throw new ArgumentNullException(nameof(requiredParameter));
 }
 ```

 **Example (Traditional Constructor/Method):**

 ```csharp
 public class MyOtherClass
 {
     public MyOtherClass(string requiredParameter)
     {
         if (requiredParameter is null)
         {
             throw new ArgumentNullException(nameof(requiredParameter));
         }
         // ...
     }

     public void ProcessData(List<string> data)
     {
         if (data is null)
         {
             throw new ArgumentNullException(nameof(data));
         }
         // ...
     }
 }
 ```

 ---

 ## Asynchronous Programming

 * **I/O-Bound Operations**: When performing I/O-bound operations (e.g., file access, network requests), use the `async` and `await` keywords with truly asynchronous APIs (e.g., `FileStream.ReadAsync`, `HttpClient.GetStringAsync`). Avoid wrapping synchronous I/O operations in `Task.Run()` as this can lead to unnecessary thread pool contention.
 * **CPU-Bound Operations**: For CPU-bound operations that might block the UI thread, use `Task.Run()` to offload the work to a thread pool thread.
 * **Async All the Way**: Once a method becomes `async`, its callers should also be `async` to avoid blocking the calling thread. Avoid `Task.Result` or `Task.Wait()` in UI threads.

 ___

## Dependency Injection and Asynchronous Operations
Dependency Injection (DI) is being implemented with the runway-related classes to improve modularity, testability, and maintainability. This approach decouples classes by providing their dependencies from an external source, rather than having the classes create their own dependencies.

To support this, async versions of the Log and FileOps classes have been created specifically for use with the new runway classes. This ensures that I/O-bound operations related to runways do not block the UI thread and adhere to the "Async All the Way" standard.

The long-term plan is to gradually migrate other areas of the application to this dependency injection and asynchronous model, applying these principles to other large classes as they are refactored.
