namespace McTimeline;

public record McTimelineItem {
    public McTimelineItem(string idKey, string title, string description, DateTime start, DateTime end) {
        IdKey = idKey;
        Title = title;
        Description = description;
        Start = start;
        End = end;
        Visible = false;
        Selected = false;
    }

    public string IdKey { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool Visible { get; set; } = false;
    public bool Selected { get; set; } = false;
}
