using TeamTaskManager.Models.Enums;
using System.Windows.Media;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;

namespace TeamTaskManager.Helpers
{
    public static class StyleHelper
    {
        private static readonly Dictionary<TaskType, (Brush Bg, Brush Fg)> TypeStyles = new()
        {
            { TaskType.Bug,     (CreateFrozenBrush(0xFE, 0xF2, 0xF2), CreateFrozenBrush(0xB9, 0x1C, 0x1C)) }, // czerwony
            { TaskType.Feature, (CreateFrozenBrush(0xEF, 0xF6, 0xFF), CreateFrozenBrush(0x1D, 0x4E, 0xD8)) }, // niebieski
            { TaskType.Task,    (CreateFrozenBrush(0xF3, 0xE8, 0xFF), CreateFrozenBrush(0x8B, 0x5C, 0xF6)) }  // fioletowy
        };

        private static readonly Dictionary<TaskStatus, (Brush Bg, Brush Fg)> StatusStyles = new()
        {
            { TaskStatus.Closed,     (CreateFrozenBrush(0xD1, 0xFA, 0xE5), CreateFrozenBrush(0x06, 0x5F, 0x46)) }, // zielony
            { TaskStatus.InProgress, (CreateFrozenBrush(0xDB, 0xEB, 0xFF), CreateFrozenBrush(0x1D, 0x4E, 0xD8)) }, // niebieski
            { TaskStatus.Open,       (CreateFrozenBrush(0xF3, 0xF4, 0xF6), CreateFrozenBrush(0x37, 0x41, 0x51)) }  // szary
        };

        private static readonly Dictionary<TaskStatus, Brush> StatusColors = new()
        {
            { TaskStatus.Closed,     CreateFrozenBrush(0x10, 0xB9, 0x81) }, // zielony
            { TaskStatus.InProgress, CreateFrozenBrush(0x60, 0xA5, 0xFA) }, // niebieski
            { TaskStatus.Open,       CreateFrozenBrush(0xD1, 0xD5, 0xDB) }  // szary
        };

        private static readonly Dictionary<TaskPriority, Brush> PriorityColors = new()
        {
            { TaskPriority.High,   CreateFrozenBrush(0xB9, 0x1C, 0x1C) }, // czerwony
            { TaskPriority.Medium, CreateFrozenBrush(0xB4, 0x53, 0x09) }, // pomaranczowy
            { TaskPriority.Low,    CreateFrozenBrush(0x16, 0xA3, 0x9C) }  // morski
        };

        private static readonly Dictionary<ScopeChange, (Brush Bg, Brush Fg)> ScopeStyles = new()
        {
            { ScopeChange.Added,    (CreateFrozenBrush(0xFE, 0xF3, 0xC7), CreateFrozenBrush(0x92, 0x40, 0x0E)) }, // pomarańczowy
            { ScopeChange.Descoped, (CreateFrozenBrush(0xFE, 0xE2, 0xE2), CreateFrozenBrush(0x99, 0x1B, 0x1B)) }  // czerwony
        };

        // fallbacki
        private static readonly Brush DefaultStyleBg = CreateFrozenBrush(0xF3, 0xF4, 0xF6);
        private static readonly Brush DefaultStyleFg = CreateFrozenBrush(0x2B, 0x2B, 0x2B);
        private static readonly Brush DefaultColor = CreateFrozenBrush(0xD1, 0xD5, 0xDB);

        public static (Brush Bg, Brush Fg) GetTypeStyle(TaskType? type)
        {
            if (type.HasValue && TypeStyles.TryGetValue(type.Value, out var style))
                return style;
            return (DefaultStyleBg, DefaultStyleFg);
        }

        public static (Brush Bg, Brush Fg) GetStatusStyle(TaskStatus? status)
        {
            if (status.HasValue && StatusStyles.TryGetValue(status.Value, out var style))
                return style;
            return (DefaultStyleBg, DefaultStyleFg);
        }

        public static Brush GetStatusColor(TaskStatus? status)
        {
            if (status.HasValue && StatusColors.TryGetValue(status.Value, out var color))
                return color;
            return DefaultColor;
        }

        public static Brush GetPriorityColor(TaskPriority? priority)
        {
            if (priority.HasValue && PriorityColors.TryGetValue(priority.Value, out var color))
                return color;
            return DefaultColor;
        }

        public static (Brush Bg, Brush Fg) GetScopeStyle(ScopeChange? change)
        {
            if (change.HasValue && ScopeStyles.TryGetValue(change.Value, out var style))
                return style;
            return (DefaultStyleBg, DefaultStyleFg);
        }

        public static string GetStatusDisplay(TaskStatus? status) => status switch
        {
            TaskStatus.Closed => "Zamknięte",
            TaskStatus.InProgress => "W trakcie",
            TaskStatus.Open => "Otwarte",
            _ => string.Empty
        };

        public static string GetScopeDisplay(ScopeChange? scope) => scope switch
        {
            ScopeChange.Added => "+dodane",
            ScopeChange.Descoped => "descoped",
            _ => string.Empty
        };

        public static string GetInitials(string? name)
        {
            return string.IsNullOrWhiteSpace(name) ? "?"
                   : string.Concat(name.Split(' ').Select(n => n[0])).ToUpper();
        }

        // aby nie obliczalo za kazdym propertychange
        private static Brush CreateFrozenBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}