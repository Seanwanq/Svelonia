# Gemini 开发日志 - 2026-01-04

## 核心总结：无限画布架构与高性能图形引擎化

今天我们以 AvaXMind 为试验田，完成了一次从“基础 UI 框架”向“高性能图形应用引擎”的架构级跃迁。通过引入 GPU  矩阵变换、细粒度视口裁剪以及响应式树扁平化，Svelonia 现在能够轻松支撑万级节点的实时交互。

### 1. 架构升级：Matrix-First 交互模型

- **InfiniteCanvas 诞生**：摒弃了传统的 `ScrollViewer` 方案，改用纯 `RenderTransform` (Matrix) 驱动。实现 了零闪烁、以鼠标中心缩放的丝滑体验。
- **Svelonia.Controls 类库**：正式建立了独立的高级组件库，封装了 `InfiniteCanvas`。
- **Svelonia.Physics 类库**：初步实现了弹簧动力学 (`SpringState`)，为 UI 增加了平滑的弹性物理反馈。       

### 2. 性能巅峰：响应式裁剪、树扁平化与增量布局

- **ReactiveViewport (核心工具)**：实现了标准化的视口裁剪逻辑。内置位移与缩放阈值拦截（Throttling），确保 在高频交互下依然保持极致跟手。
- **ReactiveTreeFlattener (Sve.FlattenTree)**：框架原生支持将递归树结构同步为扁平列表，自动追踪子节点增删 、展开/折叠状态。
- **StateList 结构感知**：为 `StateList<T>` 引入了 `Version` 状态，使 `Computed` 能够自动感知集合结构的变 化。
- **增量布局 (Local Layout)**：验证了局部响应式传播在布局计算中的威力，将 O(N) 复杂度优化为局部 O(1)。    

### 3. 技术挑战与深度修复 (Troubleshooting)

- **DevTools 递归死循环**：通过引入 **`State.SetSilent()`** 成功切断了 `StateList` 变更导致的日志递归链。 
- **AOT 资源解析异常**：改用 **Control 索引器赋值**，确保 `DynamicResourceExtension` 在 Native AOT 裁剪环 境下依然有效。
- **虚拟化缓存失效**：引入 **数据实例引用校验**，解决了添加新节点后视口不刷新的问题。
- **焦点同步“踏空”**：增强 **`BindFocus`** 监听 `IsLoaded/Attached` 事件，实现了物理焦点与状态同步的无缝排队。

---

## 🚩 AvaXMind 当前已知问题与挑战

虽然基础架构已搭建完成，但在深度交互体验上仍有以下待解决问题：

1.  **(已解决)节点操作的高延迟（1 秒响应时差）**：

    - **现状**：即便是节点极少的情况下，点击添加节点或插入父节点，UI 也会出现明显的停顿（约 1 秒）后才出现新节点和动画。优先级：**最高（紧急）**。
    - **诊断**：怀疑是 `SpringState` 动画、`EnsureVisible` 视口跟踪以及 `ReactiveViewport` 筛选逻辑之间形 成了响应式反馈循环（Feedback Loop）。

2.  **（已解决）视口自动跟踪 (EnsureVisible) 失效或不顺滑**：

    - **现状**：用户反馈该功能目前似乎未起作用。即便在节点靠近边缘时，画布也没有自动平移以保持焦点。      
    - **目标**：排查 `EnsureVisible` 的触发条件，引入更坚决的平滑移动逻辑，并解决与手动平移操作的冲突。   

3.  **（已解决）多选与批量操作逻辑增强**：

    - **Ctrl+左键**：需要支持 `Ctrl + 左键` 选中多个节点。
    - **删除回退逻辑**：多选删除后，应自动选中受影响的所有父节点。（如果对应多个父节点，则全部选中；如果仅对应一个，则选中该父节点）。

4.  **（已解决）启动时的视图瞬跳 (Glitch)**：

    - **现状**：应用启动时，思维导图有概率先以极小的尺寸出现在左上角，然后瞬间跳回。
    - **原因**：`OnLoaded` 触发时 `Bounds` 可能尚未稳定，导致 `ZoomToFit` 的初次计算基于错误的尺寸，随后被 `SizeChanged` 修正。

5.  **全局纵向导航直觉偏差**：
    - **现状**：`Up/Down` 键在跨越分支时的手感仍不够自然。

---

## 框架改进点清单 (To-Improve List)

### 核心逻辑类

- [x] **State 缓冲区 API**：内置“编辑-确认-取消”逻辑。
- [x] **ReactiveTreeFlattener**：自动将递归树结构同步为扁平列表。
- [ ] **异步状态锁**：在 `State` 变更时增加可选的并发控制。
- [ ] **性能分析工具**：在 `StateDebug` 中增加“依赖环检测”。

### Fluent API 扩展

- [x] **通用事件封装**：提供统一的 `.OnXXX` 链式封装。
- [x] **通用按下反馈**：通过伪类模拟支持所有控件。
- [x] **多态 Each 支持**：扩展 `Sve.Each` 与 `MapToChildren` 兼容 `IEnumerable<T>`。
- [x] **布局快捷键**：增加针对 Canvas 的 `.BindPosition(x, y)` 快捷方式。
- [ ] **双向绑定优化**：重构 `BindTwoWay`，使其不再依赖反射 `Value` 属性。
- [x] **CommandShortcut API**：提供语义化的、声明式的键盘快捷键绑定。

### 场景组件 (Svelonia.Controls)

- [x] **InfiniteCanvas**：支持平移、缩放、触控优化的无限画布。
- [x] **ReactiveViewport**：标准化的视口裁剪/虚拟化工具。
- [x] **SelectionManager**：通用的框选与空间点击流管理。
- [ ] **LinkEngine**：提供高性能的节点间连线渲染与路径计算。

### 开发者体验 (DX) 与 性能

- [x] **响应式裁剪插件**：已通过 `ReactiveViewport` 实现。
- [x] **高性能 Each 模式**：通过 `MapToChildren` 底层 Diff 实现。
- [ ] **Source Generator 增强**：探索通过源码生成器简化属性定义的可能性。
- [ ] **自动日志注入**：允许全局开启 `State` 变更追踪。

---

# Gemini 开发日志 - 2026-01-25

## 核心总结：架构下沉与层级自动化

今天我们将 AvaXMind 中沉淀的高级模式成功下沉到了 Svelonia 核心框架中，显著提升了框架处理复杂树形图形应用的能力。

### 1. 核心框架下沉 (Framework Sinking)

- **HierarchyStateList<T> (Svelonia.Core)**：
    - **自动关系维护**：引入了层级感知集合。当节点进入或离开 Children 列表时，框架会自动维护其 `Parent` 状态。
    - **IHierarchyNode 接口**：建立了标准的层级节点协议，使跨组件的树形逻辑复用成为可能。
    - **性能优势**：通过将 Parent/Level 的维护移至集合操作层，将导航时的查询复杂度降至极致。

- **InfiniteCanvas 增强 (Svelonia.Controls)**：
    - **Asymmetric Padding (Thickness)**：`EnsureVisible` 现在支持非对称边距。完美解决了应用级 UI（如带有顶部工具栏、底部状态栏）在进行镜头跟踪时的视觉偏移问题。

- **Fluent API 大统一 (Svelonia.Fluent)**：
    - **Sve 前缀规范**：为了彻底解决与源码生成器的命名冲突，手动扩展方法统一采用 `SveBindXXX` / `SveSetXXX` 命名规范。
    - **语法一致性**：彻底解决了在链式调用中必须切换回 Avalonia 原生 `Bind`（返回 IDisposable）而导致的断链问题。

- **源码生成器增强 (Svelonia.Gen)**：
    - **命名空间扩展**：新增对 `Avalonia.Controls.Shapes` 的支持，使 Rectangle, Path 等形状控件也能享受流式绑定。
    - **访问权限优化**：生成的类改为 `public`，确保在复杂的项目依赖结构中依然保持可见性。

### 2. 交互逻辑精进：逻辑辅助的空间导航 (LBSN)

- **算法公式**：`Score = |dy| + (dx * 3.0) + (levelDiff * 40)`。
- **直觉对齐**：通过大幅加权水平偏移和层级差异，解决了思维导图在节点密集区域“乱跳”的直觉偏差问题。
- **主动镜头同步**：由原本的“响应式监听”改为“指令式立即同步”，解决了在 10k 节点负载下由于响应式调度延迟导致的镜头跟丢问题。

### 3. 大规模数据性能 (Massive Data)

- **10k 节点秒级生成**：通过 `HierarchyStateList` 的手动构建与批量激活模式，避开了初始化时的布局风暴。
- **日志限流 (Log Throttling)**：通过根据节点总数自动屏蔽明细日志，解决了控制台 I/O 导致的 UI 线程冻结。

---

## 🚩 当前已知问题与挑战
...