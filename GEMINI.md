# Svelonia Project Status (GEMINI.md)

> 此文档汇总了 Svelonia 框架的当前状态、各模块已实现功能及待解决问题。
> 最后更新：2026-01-31

---

## 📂 项目结构

请参见 `src/` 下各子模块。

```text
.
├── src
│   ├── Svelonia.Core       # 响应式内核 (State, Computed, Buffer)
│   ├── Svelonia.Fluent     # 链式 UI 构建与绑定
│   ├── Svelonia.Controls   # 高性能组件 (InfiniteCanvas)
│   ├── Svelonia.Physics    # 物理引擎 (SpringState)
│   ├── Svelonia.Wasi       # WASM 插件宿主 (Wasmtime, AOT)
│   ├── Svelonia.Gen        # Source Generators (AOT Bindings)
│   └── Svelonia.Kit        # 应用框架 (Routing, Theming)
├── demo
│   └── AvaXMind            # 旗舰演示应用 (思维导图)
└── docs                    # 文档中心
    ├── tutorials           # 实战教程
    └── Svelonia.XXX        # 各模块 API 参考
```

---

## 🧩 模块状态汇总 (Module Status)

### 1. 核心与响应式 (Core / Fluent)

**✅ 已实现 (Completed)**
*   **Fine-Grained Reactivity**: `State<T>`, `Computed<T>`, `Effect`。支持 Dirty Bit 传播和依赖自动追踪。
*   **StateList & Hierarchy**: `StateList<T>` 支持结构感知；`HierarchyStateList<T>` 自动维护 Parent/Child 引用。
*   **Fluent API**: 全面覆盖 Avalonia 控件属性 (`.SetWidth`, `.OnClick`)。
*   **Universal Styling**: 支持 `.WhenHovered`, `.WhenPressed` 等伪类样式，且支持原子化样式 (`.P(2)`, `.Bg()`)。
*   **Bindings**: 强类型双向绑定，支持 `BufferedState`（编辑暂存模式）。

**🚧 待改进 (To-Improve)**
*   **Object Pooling**: 尚未实现控件对象池，高频创建销毁有 GC 压力。
*   **Async State Lock**: 在多线程环境下对 `State` 变更的并发控制。
*   **Style Generator**: 考虑用 Source Generator 进一步减少 Style 定义的样板代码。

### 2. 高性能图形 (Svelonia.Controls)

**✅ 已实现 (Completed)**
*   **InfiniteCanvas**: 基于 GPU `RenderTransform` (Matrix) 的无限画布，支持平移/缩放，无 Layout 性能损耗。
*   **ReactiveViewport**: 响应式视口裁剪 (Culling)，支持位移/缩放阈值节流，支撑 10k+ 节点。
*   **SelectionManager**: 统一的框选与空间点击流管理。
*   **Auto-Scroll**: `EnsureVisible` 支持不对称 Padding（避让工具栏）。

**🚧 待改进 (To-Improve)**
*   **Spatial Index**: 目前采用线性遍历+视口过滤。对于 100k+ 节点，需引入 Quadtree 空间索引。
*   **LinkEngine**: 节点连线目前较为基础，密集场景下连线路径规划需要优化（避免穿过节点）。

### 3. 物理与动画 (Svelonia.Physics)

**✅ 已实现 (Completed)**
*   **SpringState**: 基于 Semi-Implicit Euler 积分的弹簧物理，支持稳定子步 (Sub-stepping)。
*   **Robustness**: 在低帧率下保持稳定，静止时自动休眠。

**🚧 待改进 (To-Improve)**
*   **Advanced Animation System**: [NEXT STEP] 引入 GPU 计算或更复杂的力导向图 (Force Directed)。
*   **Particle System**: 尚未支持大规模粒子效果。

### 4. WASI 插件系统 (Svelonia.Wasi)

**✅ 已实现 (Completed)**
*   **Modular Host**: 模块化架构 (`CoreExtension`, `DrawingExtension`, `TimeExtension`)。
*   **Memory Management**: 实现了 `svelonia_alloc/free` 协议，零内存泄漏。
*   **Type-Safe Bindings**: 自动处理 String/JSON 封送。
*   **Native AOT Compatible**: 
    *   移除所有 Runtime Reflection。
    *   利用 `Svelonia.Gen` (Source Generator) 在编译时生成绑定代码。
    *   完全支持 `[WasiModule]`, `[WasiFunction]` 属性驱动开发。
*   **Rust Integration**: 验证了 Rust (`sticky_note.wasm`) 与 C# Host 的双向互调。

**🚧 待改进 (To-Improve)**
*   **Fuel Metering**: 因 Wasmtime 版本差异暂时禁用，需重新启用以防 DoS。
*   **Standard Library Separation**: 将通用 Extension（如 Drawing）从内核剥离到独立标准库。
*   **Binary Marshalling**: 探索 FlatBuffers 以提升高性能场景下的数据传输。

### 5. 演示应用 (AvaXMind)

**✅ 已实现 (Completed)**
*   **10k Node Performance**: 依靠视口裁剪和增量布局，实现流畅交互。
*   **Drag-to-Reparent**: 智能拖拽重组，支持“让位动画”和“虚影预测”。
*   **Theming**: 完整的亮/暗色主题切换，支持动态资源绑定。
*   **Plugins**: 集成了 Clock Demo 和 Rust Sticky Note 插件。

**🚧 待改进 (To-Improve)**
*   **Multi-Select Operations**: 多选拖拽重组逻辑尚未完善。
*   **Startup Glitch**: 初始加载时的 ZoomToFit 偶发跳动。

---

## 📅 下一步计划 (Roadmap)

### Phase 1: Advanced Animation System (当前重点)
目标：引入 GPU 加速计算与复杂的物理动力学模拟。
*   **技术选型**: 对比 Compute Shaders vs Direct Skia vs WASM SIMD。
*   **功能目标**: 实现大规模且平滑的节点布局动画与粒子特效。

### Phase 2: System Polish
*   完善文档 (API Docs 覆盖率)。
*   Style System 视觉细节打磨。

---

## 📜 历史里程碑 (Archived Log Summary)
*   **2026-01-29**: WASI Plugin System (AOT, Rust, Source Gen) 完成。
*   **2026-01-25**: 10k 节点性能优化 (Hierarchy Automation, Drag-Reparent)。
*   **2026-01-13**: 物理引擎稳定性 (Robust Sub-stepping)。
*   **2026-01-04**: InfiniteCanvas 架构与 ReactiveViewport。
*   **2026-01-03**: 响应式内核 (Active/Silent Updates)。