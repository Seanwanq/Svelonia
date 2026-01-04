# Reactivity Mastery: The Mental Model

Svelonia's reactivity is "fine-grained," inspired by Svelte and SolidJS. To use it effectively, you must understand how it tracks dependencies under the hood.

## 1. The "Observer Context" Rule

Reactivity in Svelonia is **not magic**; it is based on a stack-based tracking system called `ObserverContext`.

### The Golden Rule
> **Dependencies are only tracked when a `.Value` is READ inside a tracking scope (`Computed` or `Effect`).**

```csharp
var count = new State<int>(0);

// CORRECT: Read .Value inside the lambda
var doubled = new Computed<int>(() => count.Value * 2); 

// WRONG: This lambda does not read .Value, so it will never update
var broken = new Computed<int>(() => 42); 
```

## 2. Common Pitfall: The "Loop" Trap

When iterating over a list inside a `Computed`, you might accidentally create thousands of dependencies.

### Example: Filtering a List
```csharp
var visibleItems = new Computed<IEnumerable<Item>>(() => {
    // This tracks the list structure (Count/Items)
    var all = myStateList.Value; 
    
    return all.Where(item => {
        // WARNING: If you read item.X.Value here, this Computed 
        // will re-run whenever ANY item moves!
        return item.X.Value > 0; 
    });
});
```

### The Solution: `Sve.Untrack`
Use `Sve.Untrack` to read values that you need for logic but don't want to "subscribe" to.

```csharp
return Sve.Untrack(() => all.Where(...).ToList());
```

## 3. Directional Flow

*   **Bottom-Up (Size)**: Child changes size -> Parent recalculates its bounds.
*   **Top-Down (Position)**: Parent moves -> Child updates its absolute coordinates.

Avoid **Circular Dependencies**: If A depends on B, and B depends on A, Svelonia will detect the cycle and prevent an infinite loop, but your UI will stop updating.

## 4. The "Dirty Bit" Mechanism

Svelonia uses a "Dirty Bit" to handle high-frequency synchronous updates. If you update a `State` 100 times in a single frame, a `Computed` value will only recalculate **once** (lazily) when it is next accessed or rendered.

## 5. Structural Tracking with `StateList`

Unlike standard collections, `StateList<T>` integrates with the reactivity system. When you use a `StateList` inside a `Computed` block (e.g., calling `.Count` or iterating), Svelonia tracks the **collection structure**.

Any `Add`, `Remove`, or `Clear` operation on the list will automatically invalidate the dependent `Computed` values.

---

## Summary Checklist for Developers

1.  [ ] Did I read `.Value` for every dependency I need?
2.  [ ] Am I reading `.Value` inside a loop? (Consider `Untrack`)
3.  [ ] Is my `Computed` logic pure? (Avoid side effects inside `Computed`)
4.  [ ] If using `StateList`, did I use `Version.Value` to track structural changes?
