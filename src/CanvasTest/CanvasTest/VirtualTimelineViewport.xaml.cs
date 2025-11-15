using Microsoft.UI;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Foundation;

namespace CanvasTest;

public sealed partial class VirtualTimelineViewport : UserControl
{
    // ===== Eix temporal =====
    private readonly VirtualTimeAxis _timeAxis = new();

    public DateTime MinDate = new DateTime(2025, 1, 1);
    public DateTime MaxDate = new DateTime(2025, 12, 31);


    // ===== Dades =====
    private readonly List<TimelineItem> _items = new();     // tots
    private readonly List<TimelineItem> _visible = new();   // visibles (actualització a RealizeVisible)

    // ===== UI =====
    private readonly Dictionary<string, Border> _realized = new(); // KeyId -> UI
    private readonly Stack<Border> _pool = new();

    // ===== Estat =====
    private bool _suppressScrollEvent;
    private bool _autoFitPixelsPerHour;

    // ===== Config =====
    public double PreloadBufferPixels { get; set; } = 200.0;
    private const double ItemHeight = 20.0;
    private const double MinPixelWidth = 1.0;

    public VirtualTimelineViewport()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        ViewportHost.SizeChanged += (_, __) => MeasureAndUpdate();
        PointerWheelChanged += OnPointerWheel;
    }

    // ---------------- PUBLIC API ----------------

    public void SetDateRange(DateTime min, DateTime max, double pixelsPerHour = 6.0)
    {
        _timeAxis.SetRange(min, max);
        if (pixelsPerHour == 0)
        {
            _autoFitPixelsPerHour = true;
        }
        else
        {
            _autoFitPixelsPerHour = false;
            _timeAxis.PixelsPerHour = Math.Max(0.01, pixelsPerHour);
        }
        MeasureAndUpdate();
    }

    public void SetItems(IEnumerable<TimelineItem> items)
    {
        ClearAllVisuals(alsoClearPool: true);
        _items.Clear();
        _items.AddRange(items.OrderBy(i => i.Start));
        MeasureAndUpdate();
    }

    /// <summary>Exposa la llista d'ítems visibles (snapshot de l'últim Realize).</summary>
    public IReadOnlyList<TimelineItem> VisibleItems => _visible;

    // ---------------- EVENTS / LIFECYCLE ----------------

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // DEMO: si no hi ha res, muntem una línia temporal d'exemple
        if (_items.Count == 0)
        {
            SetDateRange(MinDate, MaxDate, pixelsPerHour: 55.0 / 24.0);
            var demo = MockTimelineItems.Generate(100, MinDate, MaxDate);
            SetItems(demo);
        }
    }

    private void OnPointerWheel(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
        var point = e.GetCurrentPoint(ViewportHost).Position;
        double screenX = point.X;

        bool isCtrlPressed = (e.KeyModifiers & Windows.System.VirtualKeyModifiers.Control) != 0;


        if (isCtrlPressed)
        {
            // Zoom
            double zoomFactor = delta > 0 ? 1.1 : 0.9;
            double newPixelsPerHour = Math.Clamp(_timeAxis.PixelsPerHour * zoomFactor, 0.01, 1000.0);

            // Get current time under mouse
            double worldHours = _timeAxis.ScreenToHours(screenX);

            // Update scale
            _timeAxis.PixelsPerHour = newPixelsPerHour;

            // Adjust offset to keep time under mouse
            _timeAxis.OffsetHours = worldHours - screenX / newPixelsPerHour;

            MeasureAndUpdate();
        }
        else
        {
            // Scroll
            var scrollDeltaPx = -delta; // natural
            _timeAxis.ScrollByPixels(scrollDeltaPx);

            _suppressScrollEvent = true;
            HScroll.Value = _timeAxis.OffsetHours; // hores
            _suppressScrollEvent = false;

            RealizeVisible();
        }
    }

    private void OnHScrollChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_suppressScrollEvent)
            return;
        _timeAxis.OffsetHours = e.NewValue;  // hores
        RealizeVisible();
    }

    // ---------------- LAYOUT / SCROLLBAR ----------------

    private void MeasureAndUpdate()
    {
        // Clip
        ViewportHost.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, ViewportHost.ActualWidth, ViewportHost.ActualHeight)
        };

        _timeAxis.ViewportPixels = Math.Max(0, ViewportHost.ActualWidth);

        if (_autoFitPixelsPerHour)
        {
            double hours = (_timeAxis.MaxDate - _timeAxis.MinDate).TotalHours;
            if (hours > 0)
            {
                _timeAxis.PixelsPerHour = _timeAxis.ViewportPixels / hours;
            }
            _autoFitPixelsPerHour = false;
        }

        // ScrollBar en hores
        _suppressScrollEvent = true;
        HScroll.Minimum = 0;
        HScroll.Maximum = _timeAxis.MaxOffsetHours;
        HScroll.ViewportSize = _timeAxis.ViewportHours;

        // Fes que tecles/“page” tinguin sentit en hores
        HScroll.SmallChange = 1; // 1 hora
        HScroll.LargeChange = Math.Max(7, Math.Round(_timeAxis.ViewportHours)); // "pàgina"

        HScroll.Value = _timeAxis.OffsetHours;
        _suppressScrollEvent = false;

        RealizeVisible();
    }

    // ---------------- VIRTUALITZACIÓ ----------------

    private void RealizeVisible()
    {
        if (_timeAxis.ViewportPixels <= 0)
            return;

        // Converteix el buffer de píxels a hores
        double bufferHours = PreloadBufferPixels / Math.Max(_timeAxis.PixelsPerHour, 1e-6);

        // 1) Calcula quins són visibles (amb buffer)
        var (leftHours, rightHours) = _timeAxis.VisibleWorldRange; // hores
        var targetVisibleIds = new HashSet<string>();
        _visible.Clear();

        foreach (var it in _items)
        {
            // Intersecció amb el viewport (en hores, amb buffer)
            if (_timeAxis.Intersects(it.Start, it.End, bufferHours))
            {
                targetVisibleIds.Add(it.IdKey);
                _visible.Add(it);
            }
        }

        // 2) Allibera els que ja no toquen
        if (_realized.Count > 0)
        {
            var toRelease = new List<string>();
            foreach (var kv in _realized)
            {
                if (!targetVisibleIds.Contains(kv.Key))
                    toRelease.Add(kv.Key);
            }
            foreach (var id in toRelease)
                ReleaseElement(id);
        }

        // 3) Garanteix visuals per tots els visibles i posiciona'ls
        foreach (var it in _visible) // iterem només visibles
        {
            if (_realized.TryGetValue(it.IdKey, out var el))
            {
                UpdateElement(el, it);
                PositionElement(el, it);
            }
            else
            {
                var elNew = AcquireElement();
                UpdateElement(elNew, it);
                CanvasHost.Children.Add(elNew);
                _realized[it.IdKey] = elNew;
                PositionElement(elNew, it);
            }
        }
    }

    // ---------------- POOL ----------------

    private Border AcquireElement()
    {
        if (_pool.Count > 0)
        {
            var el = _pool.Pop();
            el.Visibility = Visibility.Visible;
            return el;
        }
        return new Border
        {
            Height = ItemHeight,
            CornerRadius = new CornerRadius(5),
            BorderThickness = new Thickness(1)
        };
    }

    private void ReleaseElement(string id)
    {
        if (!_realized.TryGetValue(id, out var el))
            return;

        _realized.Remove(id);
        CanvasHost.Children.Remove(el);

        el.Tag = null;
        el.Visibility = Visibility.Collapsed;

        _pool.Push(el);
    }

    private void ClearAllVisuals(bool alsoClearPool)
    {
        foreach (var el in _realized.Values)
            CanvasHost.Children.Remove(el);
        _realized.Clear();

        if (alsoClearPool)
            _pool.Clear();
    }

    // ---------------- RENDERING D'ITEMS ----------------

    private void UpdateElement(Border el, TimelineItem it)
    {
        // Clamp temporal al rang global
        var s = it.Start < _timeAxis.MinDate ? _timeAxis.MinDate : it.Start;
        var e = it.End > _timeAxis.MaxDate ? _timeAxis.MaxDate : it.End;
        if (e < s)
            (s, e) = (e, s);

        var wHours = (e - s).TotalHours;
        var wPx = Math.Max(MinPixelWidth, wHours * _timeAxis.PixelsPerHour);
        el.Width = wPx;

        // Set border color based on position in the full items list
        if (it.IdKey == _items.First().IdKey)
        {
            el.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Green);
        }
        else if (it.IdKey == _items.Last().IdKey)
        {
            el.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Red);
        }
        else
        {
            el.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.SteelBlue);
        }

        //el.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.SteelBlue);
        el.Background = null;
        el.Tag = it.IdKey;
        //el.ToolTip = $"{it.Title}\n{s:d} → {e:d}";
    }

    private void PositionElement(FrameworkElement el, TimelineItem it)
    {
        // La X és el "start" clampat
        var s = it.Start < _timeAxis.MinDate ? _timeAxis.MinDate : it.Start;
        var xPx = _timeAxis.TimeToScreen(s);
        Canvas.SetLeft(el, xPx);
        Canvas.SetTop(el, 10);
    }
}
