namespace McTimeline.Pools;

/// <summary>
/// Provides a generic object pool for reusable UI elements to minimize allocations
/// and improve rendering performance.
/// </summary>
/// <typeparam name="T">The type of element to pool. Must be a FrameworkElement with a parameterless constructor.</typeparam>
public class McElementPool<T> : IDisposable where T : FrameworkElement, new()
{
    private const int INITIAL_POOL_SIZE = 10;
    
    private readonly Stack<T> _pool;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the style to apply to pooled elements.
    /// </summary>
    public Style? ItemStyle { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McElementPool{T}"/> class.
    /// </summary>
    /// <param name="itemStyle">The style to apply to created elements.</param>
    /// <param name="initialSize">The initial number of elements to pre-create in the pool.</param>
    public McElementPool(Style? itemStyle = null, int initialSize = INITIAL_POOL_SIZE) {
        _pool = new Stack<T>(initialSize);
        ItemStyle = itemStyle;
        
        for (int i = 0; i < initialSize; i++) {
            _pool.Push(CreateElement());
        }
    }

    /// <summary>
    /// Gets an element from the pool, creating a new one if the pool is empty.
    /// </summary>
    /// <returns>A reusable element instance.</returns>
    public T GetElement() {
        T element = _pool.Count > 0 ? _pool.Pop() : CreateElement();
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
    /// Creates a new element instance.
    /// </summary>
    private T CreateElement() {
        T element = new();
        
        if (ItemStyle != null) {
            element.Style = ItemStyle;
        }
        
        return element;
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