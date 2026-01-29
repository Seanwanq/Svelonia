# Tutorial: Building a High-Performance WASI Plugin with Rust

This comprehensive guide transforms you from a C# developer into a polyglot plugin architect. We will build a high-performance **Rust** plugin that integrates seamlessly with your **Svelonia** host application.

**Prerequisites**:
- [Rust Toolchain](https://www.rust-lang.org/tools/install) installed.
- Svelonia Host Application configured (see [Wasi API Reference](../Svelonia.Wasi/README.md)).

---

## 🏗️ Step 1: Rust Project Setup

Svelonia plugins are standard WebAssembly modules targeting the `wasm32-wasip1` (formerly `wasm32-wasi`) interface.

1.  **Create the Project**:
    ```bash
    cargo new --lib my-plugin
    cd my-plugin
    ```

2.  **Configure `Cargo.toml`**:
    We need to tell Rust to build a dynamic system library (`cdylib`), which for the WASM target means a `.wasm` file.
    ```toml
    [package]
    name = "my-plugin"
    version = "0.1.0"
    edition = "2021"

    [lib]
    crate-type = ["cdylib"]

    [dependencies]
    # No external crates required for basic interop!
    # For advanced serialization, consider `serde`.
    ```

---

## 🛠️ Step 2: Implementing the Guest Logic

Open `src/lib.rs`. We need to handle three things:
1.  **Memory Management**: Expose an allocator so the Host can pass strings to us.
2.  **Imports**: Declare functions that the C# Host provides.
3.  **Exports**: Define functions the C# Host can call.

### 2.1 Memory Management (Boilerplate)
Copy this standard allocator shim. It maps Rust's `alloc` to a function the Host can call.

```rust
use std::alloc::{alloc, dealloc, Layout};
use std::mem;

#[no_mangle]
pub extern "C" fn svelonia_alloc(len: u32) -> *mut u8 {
    unsafe {
        let layout = Layout::from_size_align(len as usize, 1).unwrap();
        alloc(layout)
    }
}

#[no_mangle]
pub extern "C" fn svelonia_free(ptr: *mut u8) {
    // In a complex scenario, you'd track size to dealloc correctly.
    // For simple string passing, this shim is often sufficient or left empty 
    // if the Host takes ownership of return values (which Svelonia does).
}
```

### 2.2 Import Host Functions
These must match the `[WasiModule]` and `[WasiFunction]` attributes in your C# `IWasiExtension`.

```rust
#[link(wasm_import_module = "svelonia")]
extern "C" {
    // Corresponds to [WasiFunction("add")]
    fn add(a: f64, b: f64);
    
    // Corresponds to [WasiFunction("log")]
    // Strings are passed as (pointer, length) pairs
    fn log(ptr: *const u8, len: usize);
}

// Helper wrapper for safe string passing
fn console_log(msg: &str) {
    unsafe { log(msg.as_ptr(), msg.len()); }
}
```

### 2.3 Export Plugin Functions
This is your plugin's public API.

```rust
#[no_mangle]
pub extern "C" fn run_calculation() {
    console_log("Hello from Rust! Calling C# Add...");
    
    unsafe {
        add(10.5, 20.0);
    }
    
    console_log("Calculation requested via Host.");
}
```

---

## 📦 Step 3: Compiling to WASM

Compile the project targeting WASI.

```bash
cargo build --target wasm32-wasip1 --release
```

**Output Location**:
`target/wasm32-wasip1/release/my_plugin.wasm`

*Tip: If you don't have the target installed, run `rustup target add wasm32-wasip1`.*

---

## 🔌 Step 4: Loading in C#

Now switch back to your Svelonia application.

1.  **Copy the WASM file** to your output directory (or configure `.csproj` to copy it).
2.  **Load and Run**:

```csharp
using Svelonia.Wasi;

// ... inside your Page or ViewModel ...

public void RunPlugin()
{
    // 1. Initialize Host (Global or Scoped)
    var host = new WasiHost(); 
    
    // 2. Load the specific WASM file
    // Define permissions if your extensions require them
    using var plugin = host.LoadPlugin("my_plugin.wasm", permissions: new[] { "calc" });
    
    // 3. Call the exported Rust function
    plugin.Call("run_calculation");
}
```

## 🔍 Next Steps

- Learn about the **API Architecture** in the [Svelonia.Wasi Reference](../Svelonia.Wasi/README.md).
- Explore **Complex Types** by serializing JSON strings across the boundary.
- Check `demo/sdk/rust` in the repository for a complete working example (Sticky Note Demo).
