using McTimeline;

namespace McTimelineDemo;

internal class MockTimelineItemsSeries
{
    public static IEnumerable<McTimelineItem> Generate(int maxItemsCount, DateTime startDate, DateTime endDate)
    {
        var items = new List<McTimelineItem>();
        var random = new Random();
        var currentStartDate = startDate;
        var currentEndDate = endDate;
        while (currentStartDate <= endDate && items.Count() < maxItemsCount)
        {
            var duration = random.Next(8, 24); // durada entre 8 i 24 hores
            var itemEndDate = currentStartDate.AddHours(duration);
            if (itemEndDate > endDate)
            {
                itemEndDate = endDate;
            }
            var item = new McTimelineItem
            (
                Guid.NewGuid().ToString(),
                $"Item {items.Count + 1}",
                $"Description for item {items.Count + 1}",
                currentStartDate,
                itemEndDate
            );
            items.Add(item);
            // el proper item comença entre 2 i 6 hores després de l'anterior
            var gap = random.Next(2, 10);
            currentStartDate = itemEndDate.AddHours(gap);
        }
        return items.Take(maxItemsCount);
    }
}
