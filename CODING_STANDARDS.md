# App Coding Standards (P3D Scenario Generator)

---

## 1. Architecture & Dependency Injection

* **Architecture:** Layered service model with Dependency Injection.
* **Core Services:** All file operations, JSON serialization, and logging must route through centralized `FileOps` and `Logger` instances rather than direct `System.IO` static calls.
* **Modal Dialogs vs. Logging:**
  * **Background & Scenario Generation Services:** Must **never** pop modal `MessageBox` dialogs or block execution. Errors and status must route strictly through `_logger` and `_progressReporter`.
  * **Interactive User Input Workflows:** When a user is directly interacting with a modal file-picker or action prompt (such as selecting an aircraft variant or confirming a deletion), displaying a modal `MessageBox` is permitted for actionable validation errors (e.g., invalid folder structure, missing panel folder) to ensure immediate user acknowledgement. Such errors must **also** be recorded to `_logger`.
* **Fail-Fast & Return Values:** Never silently discard the return value of fallible operations (e.g., `FileOps` methods or boolean `Try...` methods). If an operation fails:
  * Check the `bool` or tuple return value.
  * Log the failure using `_logger`.
  * Either abort the scenario workflow cleanly or report the failure gracefully to the user via `_progressReporter` (or modal prompt if interactive).

---

## 2. Asynchronous Programming (Desktop UI Model)

* **UI Responsiveness:** The WinForms UI thread must never freeze. Long-running or blocking work must be awaited.
* **Native Async I/O:** Use true async APIs where available (`FileStream.ReadAsync`, `HttpClient`, `JsonSerializer.DeserializeAsync`).
* **Sync I/O Offloading:** For file system operations that lack native asynchronous .NET APIs (such as `File.Copy`, `File.Delete`, and `Directory.CreateDirectory`), wrapping them in `Task.Run()` inside `FileOps` is standard practice to prevent desktop UI thread stutter.
* **Async All the Way:** Methods calling asynchronous APIs must return `Task` or `Task<T>` and append the `Async` suffix to the method name. Never use `.Result` or `.Wait()`.

---

## 3: Parameter Validation & Null Safety

* **Nullable Context:** The project runs with `#nullable enable`. Explicitly mark nullable variables and returns with `?` (e.g., `HtmlNode?`, `T?`). Avoid the null-forgiving operator (`!`) unless safety is guaranteed by preceding logic.
* **The Boundary Rule for Guards:**
  * **Primary Constructors / Dependency Injection:** Always validate injected services using null-coalescing throw expressions:
    ```csharp
    public class ScenarioService(Logger logger, FileOps fileOps)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
    }
    ```
  * **Public Entry Points:** Validate incoming arguments (such as `ScenarioFormData`) once at the top-level orchestration method crossing the boundary from the UI:
    ```csharp
    public async Task<bool> GenerateScenarioAsync(ScenarioFormData formData)
    {
        ArgumentNullException.ThrowIfNull(formData);
        // ...
    }
    ```
  * **Internal & Private Helper Methods:** **Do not guard.** Internal calculations, coordinate algorithms, and private helpers assume parameters were already validated at the public entry point. Omit redundant guard boilerplate in internal routines.

---

## 4. Logging & UI Progress Reporting

### Internal Logging (`Logger`)
* Used for developer diagnostic and troubleshooting logs written to disk.
* Log failures, warnings, and milestone completions.
* Keep messages descriptive. Avoid hardcoding outdated class name prefixes into log text (e.g., use `FileOps.` rather than legacy names like `FileOpsAsync.`).

### User Progress (`IProgress<string>` / `FormProgressReporter`)
* Used for high-level, human-friendly status updates in the UI.
* Messages must be concise and prefixed with a severity tag when communicating status:
  * **ERROR:** Halts execution; requires user intervention or indicates a bug.
  * **WARNING:** Non-critical issue (e.g., failed cleanup, cache miss) where the workflow can still proceed.
  * **INFO:** Routine progress updates.

---

## 5: Documentation & XML Standards

### Scope of XML Documentation
* **Public & Service APIs (Mandatory):** All public classes, primary constructors, public/internal service methods, and generator orchestration routines must include XML `<summary>` and `<returns>` tags.
* **Return Values:** When documenting boolean or status returns, explain what `true` and `false` represent:
  ```csharp
  /// <returns><see langword="true"/> if the asset was generated successfully; otherwise, <see langword="false"/>.</returns>
  ```
* **Synchronization:** Ensure all `<param>` tags match current method signatures. If a parameter is added or removed during refactoring, update or remove the corresponding `<param>` tag immediately (`CS1572`, `CS1573`).

### What NOT to Document with XML
* **WinForms Event Handlers:** Do not add XML comments to standard UI event handlers (e.g., `Button_Click`, `Form_Load`, `ComboBox_SelectedIndexChanged`). Method names are self-documenting, and XML here adds unnecessary boilerplate.
* **DTO & JSON Schema Properties:** Internal data transfer models and enum values (e.g., `Bsc5pJsonStar`, `Season`) do not require XML comments on every individual field. Document the enclosing class or enum if needed, but avoid trivial field-level comments.
* **Simple Private Methods:** Prefer clean, self-describing code over redundant XML on internal/private helpers.

### Inline Comments & AI Chat Residue
* **No Conversational Residue:** Never leave patch markers, changelog notes, or chat residue in the code (e.g., ban `// --- FIX 1: ... ---`, `// AI Note: ...`, `// Added by LLM`).
* **Document the "Why", Not the "What":** Use inline comments (`//`) sparingly, reserving them exclusively for non-obvious domain workarounds or simulator quirks (e.g., P3D coordinate bugs, OneDrive race conditions, or complex trigonometric math).
* **Format for Workarounds:**
  ```csharp
  // Workaround: OneDrive cloud sync can throw a transient IOException even when File.Move succeeded.
  ```
  
### Type Accessibility: Default to `internal`
* **Application Scope:** Because this project is a standalone desktop application (`.exe`) rather than a shared NuGet class library, types are not consumed by external assemblies.
* **Default to `internal`:** All new classes, services, records, DTOs, and enums must be declared `internal` by default rather than `public`:
  ```csharp
  internal class ScenarioFormData { ... }
  internal enum ScenarioTypes { ... }
  ```
* **When `public` is required:** Reserve `public` strictly for types mandated by the runtime or external serialization frameworks:
  * WinForms top-level forms (`public partial class Form1 : Form`).
  * Types serialized via legacy `System.Xml.Serialization.XmlSerializer` (which requires public classes and parameterless constructors).
```
