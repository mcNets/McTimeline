namespace McTimeline;

public class McTimelineItem(string idKey, string title, string description, DateTime start, DateTime end) {
    public string IdKey { get; private set; } = idKey;
    public string Title { get; private set; } = title;
    public string Description { get; private set; } = description;
    public DateTime Start { get; set; } = start;
    public DateTime End { get; set; } = end;
    public bool Visible { get; set; } = false;
    public bool Selected { get; set; } = false;
}
 