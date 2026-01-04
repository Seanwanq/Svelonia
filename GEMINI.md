# Gemini 开发日志 - 2026-01-04

## 核心总结：无限画布架构与高性能图形引擎化

今天我们以 AvaXMind 为试验田，完成了一次从“基础 UI 框架”向“高性能图形应用引擎”的架构级跃迁。通过引入 GPU 矩阵变换、细粒度视口裁剪以及响应式树扁平化，Svelonia 现在能够轻松支撑万级节点的实时交互。

### 1. 架构升级：Matrix-First 交互模型
- **InfiniteCanvas 诞生**：摒弃了传统的 `ScrollViewer` 方案，改用纯 `RenderTransform` (Matrix) 驱动。实现了零闪烁、以鼠标中心缩放的丝滑体验。
- **Svelonia.Controls 类库**：正式建立了独立的高级组件库，封装了 `InfiniteCanvas`。
- **Svelonia.Physics 类库**：初步实现了弹簧动力学 (`SpringState`)，为 UI 增加了平滑的弹性物理反馈。

### 2. 性能巅峰：响应式裁剪、树扁平化与增量布局
- **ReactiveViewport (核心工具)**：实现了标准化的视口裁剪逻辑。内置位移与缩放阈值拦截（Throttling），确保在高频交互下依然保持极致跟手。
- **ReactiveTreeFlattener (Sve.FlattenTree)**：框架原生支持将递归树结构同步为扁平列表，自动追踪子节点增删、展开/折叠状态。
- **StateList 结构感知**：为 `StateList<T>` 引入了 `Version` 状态，使 `Computed` 能够自动感知集合结构的变化。
- **增量布局 (Local Layout)**：验证了局部响应式传播在布局计算中的威力，将 O(N) 复杂度优化为局部 O(1)。

### 3. 技术挑战与深度修复 (Troubleshooting)
- **DevTools 递归死循环**：`StateList.Version` 的变更触发了 `StateDebug` 日志记录，而日志本身又在修改 `StateList`。通过引入 **`State.SetSilent()`** 绕过调试通知，成功切断了递归链。
- **AOT 资源解析异常**：`DynamicResourceExtension` 在 Native AOT 下无法通过 `SetValue` 正常工作。修复方案是改用 **Control 索引器赋值**，确保资源链接在裁剪环境下依然有效。
- **虚拟化缓存失效**：由于 Culling 逻辑中过度使用了 `Untrack` 和阈值过滤，导致新节点添加时被错误忽略。修复方案是引入 **数据实例引用校验**，实现精准的缓存失效。
- **焦点同步“踏空”**：新节点创建后因 UI 未挂载导致 `Focus()` 失败。通过增强 **`BindFocus`** 监听 `IsLoaded/Attached` 事件，实现了物理焦点与状态同步的无缝排队。

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

## 历史更新记录 (2026-01-03)
- **响应式内核加固 (Dirty Bit)**
- **强制追踪 (ForceTrack)**
- **原生绑定融合 (INPC Integration)**
- **Universal Event API**