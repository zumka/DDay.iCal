using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="TimeSpan"/> class.
    /// <remarks>
    /// This class handles parsing an RFC 5545 Duration.
    /// </remarks>
    /// </summary>
    [DebuggerDisplay("{Value}")]
#if DATACONTRACT
    [DataContract(Name = "Duration", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Duration : 
        CalendarDataType,
        IDuration
    {
        #region Private Fields

        private TimeSpan m_Value;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public TimeSpan Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        #endregion

        #region Constructors

        public Duration() { }
        public Duration(TimeSpan ts)
            : this()
        {
            this.Value = ts;
        }
        public Duration(string value) : this()
        {
            CopyFrom((Duration)Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is Duration)
            {
                Duration d = (Duration)obj;
                Value = d.Value;
            }
            base.CopyFrom(obj);
        }

        public override bool Equals(object obj)
        {
            if (obj is Duration)
            {
                Duration d = (Duration)obj;
                return Value.Equals(d.Value);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool TryParse(string value, ref ICalendarDataType obj)
        {
            Match match = Regex.Match(value, @"^(?<sign>\+|-)?P(((?<week>\d+)W)|(?<main>((?<day>\d+)D)?(?<time>T((?<hour>\d+)H)?((?<minute>\d+)M)?((?<second>\d+)S)?)?))$");
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (match.Success)
            {
                int mult = 1;
                if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                    mult = -1;

                if (match.Groups["week"].Success)
                    days = Convert.ToInt32(match.Groups["week"].Value) * 7;
                else if (match.Groups["main"].Success)
                {
                    if (match.Groups["day"].Success) days = Convert.ToInt32(match.Groups["day"].Value);
                    if (match.Groups["time"].Success)
                    {
                        if (match.Groups["hour"].Success) hours = Convert.ToInt32(match.Groups["hour"].Value);
                        if (match.Groups["minute"].Success) minutes = Convert.ToInt32(match.Groups["minute"].Value);
                        if (match.Groups["second"].Success) seconds = Convert.ToInt32(match.Groups["second"].Value);
                    }
                }

                ((Duration)obj).Value = new TimeSpan(days * mult, hours * mult, minutes * mult, seconds * mult);
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            TimeSpan ts = new TimeSpan(0);
            string value = string.Empty;
            if (ts > Value)
                value = "-";

            ts = new TimeSpan(Math.Abs(Value.Ticks));
            value += "P";

            if (ts.Days > 7 &&
                ts.Days % 7 == 0 &&
                ts.Hours == 0 &&
                ts.Minutes == 0 &&
                ts.Seconds == 0)
                value += Math.Round((double)ts.Days / 7) + "W";
            else
            {
                if (ts.Days != 0)
                    value += ts.Days + "D";
                if (ts.Hours != 0 ||
                    ts.Minutes != 0 ||
                    ts.Seconds != 0)
                {
                    value += "T";
                    if (ts.Hours != 0)
                        value += ts.Hours + "H";
                    if (ts.Minutes != 0)
                        value += ts.Minutes + "M";
                    if (ts.Seconds != 0)
                        value += ts.Seconds + "S";
                }
            }

            return value;
        }

        #endregion

        #region Operators

        static public implicit operator TimeSpan(Duration value)
        {
            if (value != null)
                return value.Value;
            else return TimeSpan.MinValue;
        }

        static public implicit operator Duration(TimeSpan ts)
        {
            return new Duration(ts);
        }

        #endregion
    }
}