using System.Collections.Generic;

namespace JobScheduler.Model
{
    public class AliasName
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AliasName(string name, List<string> aliases)
        {
            Name = name;
            Aliases = aliases;
        }

        public string Name { get; set; }
        public List<string> Aliases { get; set; }
    }

    public class ImdbMovie /*  A series is also a movie*/
    {
        public ImdbMovie()
        {
            Years = new List<int?>();
            Episodes = new List<string>();
        }

        public string Name { get; set; }
        public List<int?> Years { get; set; }
        public List<string> Episodes { get; set; } 
             
    }
}