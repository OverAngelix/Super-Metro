using System.Collections.Generic;

public class Station : Location
{
        public int lineNumber;
        public int index;
        public int level;
        public int capacity;
        public List<PersonController> waitingPeople = new List<PersonController>();

        public Station(string name, float lat, float lon, int lineNumber, int index)
            : base(name, lat, lon)
        {
            this.lineNumber = lineNumber;
            this.index = index;
            this.level = 1;
            this.capacity = 50;
        }
    

}