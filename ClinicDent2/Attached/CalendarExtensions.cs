using ClinicDent2.View;
using System.Windows;

namespace ClinicDent2.Attached
{
    public static class CalendarExtensions
    {
        #region IsDayFullProperty
        public static readonly DependencyProperty IsDayFullProperty =
            DependencyProperty.RegisterAttached("IsDayFull", typeof(bool), typeof(CalendarExtensions), new FrameworkPropertyMetadata(false));

        public static bool GetIsDayFull(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDayFullProperty);
        }

        public static void SetIsDayFull(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDayFullProperty, value);
        }
        #endregion
        #region CalendarDayButtonStateProperty
        public static readonly DependencyProperty CalendarDayButtonStateProperty =
            DependencyProperty.RegisterAttached("CalendarDayButtonState", typeof(CalendarDayButtonStates), typeof(CalendarExtensions), new FrameworkPropertyMetadata(CalendarDayButtonStates.White));

        public static CalendarDayButtonStates GetCalendarDayButtonState(DependencyObject obj)
        {
            return (CalendarDayButtonStates)obj.GetValue(CalendarDayButtonStateProperty);
        }
        public static void SetCalendarDayButtonState(DependencyObject obj, CalendarDayButtonStates value)
        {
            obj.SetValue(CalendarDayButtonStateProperty, value);
        }
        #endregion
    }
}
