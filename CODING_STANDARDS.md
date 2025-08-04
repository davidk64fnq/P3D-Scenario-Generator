# Code Documentation Standards

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

**Example:**

A warning message reported to the progress bar would be formatted as:

`WARNING: The scenery.cfg file has been modified more recently than the cached runways data.`

---

## XML Comments

All methods, properties, and classes—regardless of their access modifier (public, internal, private)—must include XML documentation comments.

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

**Example:**

```csharp
// The field is prefixed with _, the parameter is not.
private readonly ToolStripStatusLabel _statusLabel = statusLabel;
```

---

## Parameter Validation

All public methods and constructors must use guard clauses to validate non-nullable parameters. This is not required for private members, as it is assumed that validation has been performed at the public-facing boundary.

**Example:**

```csharp
// Guard clause to validate a parameter in a constructor
public MyClass(string requiredParameter)
{
    // If 'requiredParameter' is null, throw an exception
    _requiredParameter = requiredParameter ?? throw new ArgumentNullException(nameof(requiredParameter));
}
```

