namespace McTimeline.Pools;

/// <summary>
/// Provides a generic object pool for reusable UI elements to minimize allocations
/// and improve rendering performance.
/// </summary>
/// <typeparam name="T">The type of element to pool. Must be a FrameworkElement.</typeparam>
public class McElementPool<T> : IDisposable where T : FrameworkElement {
    private const int INITIAL_POOL_SIZE = 50;

    private readonly Stack<T> _pool;
    private readonly Func<T> _factory;
    private bool _disposed;
    private Style? _itemStyle;

    /// <summary>
    /// Gets or sets the style to apply to pooled elements.
    /// </summary>
    public Style? ItemStyle {
        get => _itemStyle;
        set {
            if (value != null && value.TargetType != null && !value.TargetType.IsAssignableFrom(typeof(T))) {
                throw new ArgumentException($"Style TargetType '{value.TargetType.Name}' is not compatible with element type '{typeof(T).Name}'.");
            }
            _itemStyle = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McElementPool{T}"/> class with a factory function.
    /// </summary>
    /// <param name="factory">A function that creates new instances of T.</param>
    /// <param name="itemStyle">The style to apply to created elements.</param>
    /// <param name="initialSize">The initial number of elements to pre-create in the pool.</param>
    public McElementPool(Func<T> factory, Style? itemStyle = null, int initialSize = INITIAL_POOL_SIZE) {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _pool = new Stack<T>(initialSize);
        ItemStyle = itemStyle;

        for (int i = 0; i < initialSize; i++) {
            _pool.Push(CreateElement());
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McElementPool{T}"/> class.
    /// Only works if T has a parameterless constructor.
    /// </summary>
    /// <param name="itemStyle">The style to apply to created elements.</param>
    /// <param name="initialSize">The initial number of elements to pre-create in the pool.</param>
    public McElementPool(Style? itemStyle = null, int initialSize = INITIAL_POOL_SIZE) 
        : this(() => Activator.CreateInstance<T>(), itemStyle, initialSize) {
    }

    /// <summary>
    /// Creates a new element instance.
    /// </summary>
    private T CreateElement() {
        T element = _factory();
        element.Style ??= ItemStyle;
        return element;
    }

    /// <summary>
    /// Gets an element from the pool, creating a new one if the pool is empty.
    /// </summary>
    /// <returns>A reusable element instance.</returns>
    public T GetElement() {
        ObjectDisposedException.ThrowIf(_disposed, this);

        T element = _pool.Count > 0 ? _pool.Pop() : CreateElement();

        // Ensure the element has the current style (in case ItemStyle changed)
        if (ItemStyle != null && element.Style != ItemStyle) {
            element.Style = ItemStyle;
        }

        return element;
    }

    /// <summary>
    /// Returns an element to the pool for reuse.
    /// </summary>
    /// <param name="element">The element to recycle.</param>
    public void RecycleElement(T element) {
        if (element == null) return;

        // Reset common properties
        element.Tag = null;
        element.DataContext = null;

        _pool.Push(element);
    }

    /// <summary>
    /// Clears all elements from the pool.
    /// </summary>
    public void Clear() {
        _pool.Clear();
    }

    /// <summary>
    /// Disposes the pool and clears all elements.
    /// </summary>
    public void Dispose() {
        if (!_disposed) {
            Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}