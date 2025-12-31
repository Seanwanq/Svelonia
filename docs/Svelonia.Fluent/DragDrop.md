# Drag & Drop 拖拽功能

Svelonia 提供了一套优雅且高性能的 Fluent API 来处理拖拽交互。它分为两种模式：**系统级拖拽 (Standard)** 和 **实时交互拖拽 (Live)**。

## 1. 系统级拖拽 (`Draggable`)

适用于传统的“抓取并放下”场景，支持跨应用数据传输（如拖出文件、文本）。

### 核心特性
*   **Ghost 引擎**：自动生成控件的半透明虚像跟随鼠标。
*   **视觉模式**：支持 `Ghost` (占位) 和 `Move` (物体随鼠标消失)。
*   **拖拽手柄**：支持指定子控件作为触发点。
*   **跨应用**：支持将数据拖拽到记事本、资源管理器等外部程序。

### 基本用法
```csharp
new Border()
    .W(100).H(100).Background(Brushes.SkyBlue)
    .Draggable("我是传输的数据") 
    .OnDrop(e => {
        var data = e.Data.GetText();
        Console.WriteLine($"接收到: {data}");
    });
```

### 高级用法：带手柄的卡片
```csharp
var header = new Border().TextContent("::: 抓住这里 :::");

new StackPanel()
    .Children(
        header,
        new TextBlock().TextContent("内容区域无法直接拖动")
    )
    // 指定 header 为触发手柄，使用 Move 模式（拖动时原物体变透明）
    .Draggable(data: cardModel, handle: header, visualMode: DragVisualMode.Move);
```

---

## 2. 实时交互拖拽 (`LiveDraggable`)

适用于思维导图、白板、游戏或多点触控看板。它绕过系统 OLE 限制，支持真正的并发操作。

### 核心特性
*   **多点并发**：支持多根手指同时拖动不同的物体。
*   **硬件加速**：使用 `RenderTransform` 实现，不触发布局重绘，极其丝滑。
*   **非模态**：不会阻塞 UI 线程的其他交互。

### 基本用法
```csharp
new Border()
    .W(80).H(80).Background(Brushes.Red).RoundedFull()
    .LiveDraggable() // 立即启用实时拖拽
    .OnHover(s => s.Cursor(Cursor.Parse("Hand")));
```

### 联动逻辑 (模拟/镜像)
```csharp
new Border()
    .LiveDraggable(onMove: delta => {
        // 当这个物体移动时，让另一个物体反向移动
        anotherControl.ManualMove(-delta.X, -delta.Y);
    });
```

## API 参考

### `Draggable` 参数表
| 参数 | 类型 | 说明 |
| :--- | :--- | :--- |
| `data` | `object` | 要传输的数据（String, Files, 或自定义对象） |
| `effect` | `DragDropEffects` | 允许的操作（Copy, Move, Link） |
| `handle` | `Control` | 可选。指定触发拖拽的子控件 |
| `enable` | `State<bool>` | 可选。响应式控制是否允许拖拽 |
| `visualMode` | `DragVisualMode` | `Ghost`(默认), `Move`(原位隐藏), `None` |
| `onStart/End` | `Action` | 拖拽生命周期回调 |

### `LiveDraggable` 参数表
| 参数 | 类型 | 说明 |
| :--- | :--- | :--- |
| `enable` | `State<bool>` | 可选。响应式控制 |
| `onMove` | `Action<Point>` | 实时移动时的偏移量回调（用于同步或约束逻辑） |
