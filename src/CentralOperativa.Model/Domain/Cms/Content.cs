using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms
{
    [Alias("Contents")]
    public class Content
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }

        public string Title { get; set; }

        public bool ShowTitle { get; set; }

        public string SubTitle { get; set; }

        public string Source { get; set; }

        public string Summary { get; set; }

        public string Text { get; set; }

        public string Link { get; set; }

        public byte LinkTarget { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public bool ShowPublishDate { get; set; }

        public int? EventCategoryId { get; set; }

        public DateTime? EventStartDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        public string EventPlace { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastEditDate { get; set; }

        public bool AllowAnonymous { get; set; }

        [Ignore]
        public virtual string Duration
        {
            get
            {
                string duration = null;

                var eventStartDateLocal = this.EventStartDate.HasValue ? (DateTime?) this.EventStartDate.Value.ToLocalTime() : null;
                var eventEndDateLocal = this.EventEndDate.HasValue ? (DateTime?)this.EventEndDate.Value.ToLocalTime() : null;

                if (eventStartDateLocal.HasValue)
                {
                    if (eventEndDateLocal.HasValue)
                    {
                        if (eventEndDateLocal.Value.Date == eventStartDateLocal.Value.Date)
                        {
                            if (eventEndDateLocal.Value.TimeOfDay == eventStartDateLocal.Value.TimeOfDay || eventEndDateLocal.Value.TimeOfDay.TotalMinutes.Equals(0))
                            {
                                if (eventEndDateLocal.Value.TimeOfDay.TotalMinutes.Equals(0))
                                {
                                    duration = string.Format(
                                        "{0} de {1}",
                                        eventStartDateLocal.Value.Day,
                                        eventStartDateLocal.Value.ToString("MMMM"));
                                }
                                else
                                {
                                    duration = string.Format(
                                        "{0} de {1} a las {2}",
                                        eventStartDateLocal.Value.Day,
                                        eventStartDateLocal.Value.ToString("MMMM"),
                                        eventStartDateLocal.Value.ToString("HH:mm"));
                                }
                            }
                            else
                            {
                                duration = string.Format(
                                    "{0} de {1} de {2} a {3}",
                                    eventStartDateLocal.Value.Day,
                                    eventStartDateLocal.Value.ToString("MMMM"),
                                    eventStartDateLocal.Value.ToString("HH:mm"),
                                    eventEndDateLocal.Value.ToString("HH:mm"));
                            }
                        }
                        else
                        {
                            if (eventStartDateLocal.Value.Month == eventEndDateLocal.Value.Month &&
                                eventStartDateLocal.Value.Year == eventEndDateLocal.Value.Year)
                            {
                                duration = !eventStartDateLocal.Value.TimeOfDay.TotalMinutes.Equals(0)
                                               ? string.Format(
                                                   "{0} al {1} de {2} de {3} a {4}",
                                                   eventStartDateLocal.Value.Day,
                                                   eventEndDateLocal.Value.Day,
                                                   eventStartDateLocal.Value.ToString("MMMM"),
                                                   eventStartDateLocal.Value.ToString("HH:mm"),
                                                   eventEndDateLocal.Value.ToString("HH:mm"))
                                               : string.Format(
                                                   "{0} al {1} de {2}",
                                                   eventStartDateLocal.Value.Day,
                                                   eventEndDateLocal.Value.Day,
                                                   eventStartDateLocal.Value.ToString("MMMM"));
                            }
                            else
                            {
                                duration = !eventStartDateLocal.Value.TimeOfDay.TotalMinutes.Equals(0)
                                               ? string.Format(
                                                   "{0} de {1} al {2} de {3} de {4} a {5}",
                                                   eventStartDateLocal.Value.Day,
                                                   eventStartDateLocal.Value.ToString("MMMM"),
                                                   eventEndDateLocal.Value.Day,
                                                   eventEndDateLocal.Value.ToString("MMMM"),
                                                   eventStartDateLocal.Value.ToString("HH:mm"),
                                                   eventEndDateLocal.Value.ToString("HH:mm"))
                                               : string.Format(
                                                   "{0} de {1} al {2} de {3}",
                                                   eventStartDateLocal.Value.Day,
                                                   eventStartDateLocal.Value.ToString("MMMM"),
                                                   eventEndDateLocal.Value.Day,
                                                   eventEndDateLocal.Value.ToString("MMMM"));
                            }
                        }
                    }
                    else
                    {
                        duration = !eventStartDateLocal.Value.TimeOfDay.TotalMinutes.Equals(0)
                                       ? string.Format("{0} de {1} a las {2}", eventStartDateLocal.Value.Day,
                                                       eventStartDateLocal.Value.ToString("MMMM"),
                                                       eventStartDateLocal.Value.ToString("HH:mm"))
                                       : string.Format("{0} de {1}", eventStartDateLocal.Value.Day,
                                                       eventStartDateLocal.Value.ToString("MMMM"));
                    }
                }

                return duration;
            }
        }
    }
}