using System;
using System.Collections.Generic;
using System.Linq;

namespace CanvasTest;

internal class MockTimelineItems
{
    public static IEnumerable<TimelineItem> Generate(int itemsCount, DateTime startDate, DateTime endDate)
    {
        var items = new List<TimelineItem>();
        var random = new Random();
        var currentStartDate = startDate;
        var currentEndDate = endDate;
        while (currentStartDate <= endDate && items.Count() < itemsCount)
        {
            var duration = random.Next(8, 24); // durada entre 8 i 24 hores
            var itemEndDate = currentStartDate.AddHours(duration);
            if (itemEndDate > endDate)
            {
                itemEndDate = endDate;
            }
            var item = new TimelineItem
            (
                Guid.NewGuid().ToString(),
                $"Item {items.Count() + 1}",
                $"Description for item {items.Count + 1}",
                currentStartDate,
                itemEndDate
            );
            items.Add(item);
            // el proper item comença entre 2 i 6 hores després de l'anterior
            var gap = random.Next(2, 10);
            currentStartDate = itemEndDate.AddHours(gap);
        }
        return items.Take(itemsCount);
    }
}
