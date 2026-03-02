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

        for (int i = 0; i < 10; random.Next(++i));

        // is has to start randomly a least 1 hours afetr the start date to let some space for the first item
        var initialGap = random.Next(1, 4);
        currentStartDate = currentStartDate.AddHours(initialGap);

        while (currentStartDate <= endDate.AddHours(-15) && items.Count() < maxItemsCount)
        {
            var duration = random.Next(8, 24); // between 8 and 24 hours
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
            // the next item starts between 2 and 6 hours after the previous one
            var gap = random.Next(2, 10);
            currentStartDate = itemEndDate.AddHours(gap);
        }
        return items.Take(maxItemsCount);
    }
}
