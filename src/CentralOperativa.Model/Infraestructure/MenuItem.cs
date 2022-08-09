using System.Collections.Generic;

namespace CentralOperativa.Infraestructure
{
    public class MenuItem
    {
        public MenuItem()
        {
            this.Items = new List<MenuItem>();
        }

        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Text { get; set; }

        public string State { get; set; }

        public int ListIndex { get; set; }

        public string IconClass { get; set; }

        public List<MenuItem> Items { get; set; }
    }
}
